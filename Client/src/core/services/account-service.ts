import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { User, UserLogin, UserRegister } from '../../types/user';
import { tap } from 'rxjs/internal/operators/tap';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http = inject(HttpClient);
  baseUrl = 'https://localhost:5001/api/';
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
  }
  logout() {
    this.currentUser.set(null);
    localStorage.removeItem('user');
  }

}
