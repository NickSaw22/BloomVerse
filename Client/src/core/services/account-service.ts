import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { User, UserLogin, UserRegister } from '../../types/user';
import { tap } from 'rxjs/internal/operators/tap';
import { environment } from '../../environments/environment.development';
import { LikesService } from './likes-service';
import { PresenceService } from './presence-service';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http = inject(HttpClient);
  private likesService = inject(LikesService);
  baseUrl = environment.apiUrl;
  private presenceService = inject(PresenceService);
  
  currentUser = signal<User | null>(null);

  register(credentials: UserRegister) {
    return this.http.post<User>(this.baseUrl + 'account/register', credentials, 
      {withCredentials: true}).pipe(
      tap((user) => {
        if (user) {
          this.setCurrentUser(user);
          this.startTokenRefreshInterval();
        }
      })
    );  
  }

  login(credentials: UserLogin) {
    return this.http.post<User>(this.baseUrl + 'account/login', credentials, 
      {withCredentials: true}).pipe(
      tap((user) => {
        if (user) {
          this.setCurrentUser(user);
          this.startTokenRefreshInterval();
        }
      })
    );
  }

  refereshToken() {
    return this.http.post<User>(this.baseUrl + 'account/refresh-token', {},
      {withCredentials: true})
  }

  startTokenRefreshInterval() {
    setInterval(() => {
      this.http.post<User>(this.baseUrl + 'account/refresh-token', {}, 
        { withCredentials: true }).subscribe({
          next: (user) => {
            if (user) {
              this.setCurrentUser(user);
            }
          },
          error: (err) => {
            console.error('Error refreshing token:', err);
            this.logout();
          }
        });
        }, 5*60*1000);
  }

  setCurrentUser(user: User) {
    user.roles = this.getRolesFromToken(user.token);
    this.currentUser.set(user);
    this.likesService.getLikeIds();
    if(this.presenceService.hubConnection?.state !== HubConnectionState.Connected){
      this.presenceService.createHubConnection(user);
    }
  }
  logout() {
    this.http.post(this.baseUrl + 'account/logout', {}, {withCredentials: true}).subscribe({
      next: () => {
        this.clearUserData();
      },
      error: (err) => {
        console.error('Error during logout:', err);
        this.clearUserData();
      }
    });
  }

  private clearUserData() {
    this.currentUser.set(null);
    localStorage.removeItem('filters');
    this.likesService.clearLikesIds();
    this.presenceService.stopHubConnection();
  }

  private getRolesFromToken(token: string): string[] {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return Array.isArray(payload.role) ? payload.role : [payload.role];
  }
}
