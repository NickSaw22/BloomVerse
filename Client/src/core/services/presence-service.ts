import { inject, Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { User } from '../../types/user';
import { environment } from '../../environments/environment.development';
import { ToastService } from './toast-service';
import { Message } from '../../types/message';

@Injectable({
  providedIn: 'root',
})
export class PresenceService {
  private toast = inject(ToastService);
  private hubUrl = environment.hubUrl;
  hubConnection?: HubConnection;
  onlineUsers = signal<string[]>([]);
  
  createHubConnection(user: User){
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();
    
    this.hubConnection.start()
      .catch(error => console.log('Error establishing connection: ', error));

    this.hubConnection.on('UserOnline', userId => {
      this.onlineUsers.update(users => [...users, userId]);
    });

    this.hubConnection.on('UserOffline', userId => {
      this.onlineUsers.update(users => users.filter(u => u !== userId));
    })

    this.hubConnection.on('GetOnlineUsers', (userIds: string[]) => {
      this.onlineUsers.set(userIds);
    });

    this.hubConnection.on('NewMessageReceived', (message: Message) => {
      this.toast.info(message.senderDisplayName + ' has sent you a new message!', 5000, message.senderImageUrl, `/members/${message.senderId}/messages`);
    });
  }

  stopHubConnection(){
    if(this.hubConnection?.state === HubConnectionState.Connected){
      this.hubConnection.stop().catch((error) => {
        console.log('Error stopping connection: ', error);
      });
    }
  }
}
