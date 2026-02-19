import { Component, inject, OnInit, signal } from '@angular/core';
import { MessageService } from '../../core/services/message-service';
import { Message } from '../../types/message';
import { PaginatedResult } from '../../types/pagination';
import { Paginator } from "../../shared/paginator/paginator";
import { DatePipe } from '@angular/common';
import { RouterLink } from "@angular/router";

@Component({
  selector: 'app-messages',
  imports: [Paginator, DatePipe, RouterLink],
  templateUrl: './messages.html',
  styleUrl: './messages.css',
})
export class Messages implements OnInit {
  private messageService = inject(MessageService);
  protected container = 'Inbox';
  protected pageNumber = 1;
  protected pageSize = 10;
  protected paginatedMessages = signal<PaginatedResult<Message> | null>(null);
  protected fetchedContainer = 'Inbox';
  tabs = [
    {
      label: 'Inbox',
      value: 'Inbox'
    },
    {
      label: 'Outbox',
      value: 'Outbox'
    }
  ]
  ngOnInit() {
    this.loadMessages();
  }
  
  loadMessages() {
    this.messageService.getMessages(this.container, this.pageNumber, this.pageSize).subscribe({
      next: response => {
        this.paginatedMessages.set(response);
        this.fetchedContainer = this.container;
      }
    });
  }

  deleteMessage(event: Event, id: string) {
    event.stopPropagation();
    this.messageService.deleteMessage(id).subscribe({
      next: () => {
        const currentMessages = this.paginatedMessages();
        if(currentMessages?.items){
          this.paginatedMessages.update(prev => {
            if (!prev) return null;
            const newItems = prev.items.filter(m => m.id !== id) || [];
            return { items: newItems, metadata: prev.metadata };
          });
        }
      }
    });
  }

  get isInbox(){
    return this.fetchedContainer === 'Inbox';
  }
  
  setContainer(container: string) {
    this.container = container;
    this.loadMessages();
  }

  onPageChanged(event: { pageNumber: number, pageSize: number }) {
    this.pageNumber = event.pageNumber;
    this.pageSize = event.pageSize;
    this.loadMessages();
  }
}
