import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { last, lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [],//[RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly title = 'BloomVerse'//signal('Client');
  private http = inject(HttpClient);
  protected members = signal<any>([]);

  async ngOnInit() {
    this.members.set(await this.getAllUsers());
  }

  async getAllUsers() {
    const url = 'https://localhost:5001/api/members';
    try{
      return lastValueFrom(this.http.get(url));
    }
    catch(error){
      console.error('There was an error!', error);
      return [];
    }
  }

}
