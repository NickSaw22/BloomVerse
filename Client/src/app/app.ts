import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { Nav } from '../layout/nav/nav';
import { AccountService } from '../core/services/account-service';
import { Home } from "../features/home/home";
import { User } from '../types/user';
@Component({
  selector: 'app-root',
  imports: [Nav, Home],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private accountService = inject(AccountService);
  protected readonly title = 'BloomVerse'//signal('Client');
  private http = inject(HttpClient);
  protected members = signal<User[]>([]);

  async ngOnInit() {
    this.members.set(await this.getAllUsers());
    this.setCurrnetUser();
  }

  setCurrnetUser(){
    const userString = localStorage.getItem('user');
    if(!userString) return;
    this.accountService.currentUser.set(JSON.parse(userString));
  }

  async getAllUsers() {
    const url = 'https://localhost:5001/api/members';
    try{
      return lastValueFrom(this.http.get<User[]>(url));
    }
    catch(error){
      console.error('There was an error!', error);
      return [];
    }
  }

}
