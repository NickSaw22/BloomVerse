import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { User, UserLogin, UserRegister } from '../../types/user';
import { tap } from 'rxjs/internal/operators/tap';
import { environment } from '../../environments/environment.development';
import { LikesService } from './likes-service';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http = inject(HttpClient);
  private likesService = inject(LikesService);
  baseUrl = environment.apiUrl;
  currentUser = signal<User | null>(null);

  register(credentials: UserRegister) {
    return this.http.post<User>(this.baseUrl + 'account/register', credentials).pipe(
      tap((user) => {
        if (user) {
          this.setCurrentUser(user);
        }
      })
    );  
  }

  login(credentials: UserLogin) {
    return this.http.post<User>(this.baseUrl + 'account/login', credentials).pipe(
      tap((user) => {
        if (user) {
          this.setCurrentUser(user);
        }
      })
    );
  }
  setCurrentUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
    this.likesService.getLikeIds();
  }
  logout() {
    this.currentUser.set(null);
    localStorage.removeItem('filters');
    localStorage.removeItem('user');
    this.likesService.clearLikesIds();
  }

}
