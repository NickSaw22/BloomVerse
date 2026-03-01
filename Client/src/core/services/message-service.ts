import { inject, Injectable, signal } from '@angular/core';
import { PaginatedResult } from '../../types/pagination';
import { Message } from '../../types/message';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment.development';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { AccountService } from './account-service';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  private baseUrl = environment.apiUrl;
  private hubUrl = environment.hubUrl;
  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  private hubConnection?: HubConnection;
  messageThread = signal<Message[]>([]);
  
  createHubConnection(otherUserId: string) {
    const currentUser = this.accountService.currentUser();
    if (!currentUser) return;

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'messages?userId=' + otherUserId, {
        accessTokenFactory: () => currentUser.token,
      })
      .withAutomaticReconnect()
      .build();
    
      this.hubConnection.start().catch((err) => console.error('Error starting message hub connection:', err));
      
      this.hubConnection.on('ReceiveMessageThread', (messages: Message[]) => {
        this.messageThread.set(messages.map((msg) => ({
          ...msg,
          currentUserSender: msg.senderId !== currentUser.id,
        })));
      });

      this.hubConnection.on('NewMessage', (message: Message) => {
        this.messageThread.update((messages) => [...messages, {
          ...message,
          currentUserSender: message.senderId !== currentUser.id,
        }]);
      });
  }

  stopHubConnection() {
    if(this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.stop().catch((err) => console.error('Error stopping message hub connection:', err));
    }
  }

  getMessages(container: string, pageNumber: number, pageSize: number) {
    let params = new HttpParams();

    params = params.append('container', container);
    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());
    return this.http.get<PaginatedResult<Message>>(`${this.baseUrl}messages`, { params });
  }

  getMessageThread(memberId: string) {
    return this.http.get<Message[]>(`${this.baseUrl}messages/thread/${memberId}`);
  }

  sendMessage(recipientId: string, content: string) {
    return this.hubConnection?.invoke<Message>('SendMessage', { recipientId, content });
  }

  deleteMessage(id: string) {
    return this.http.delete(`${this.baseUrl}messages/${id}`);
  }
}