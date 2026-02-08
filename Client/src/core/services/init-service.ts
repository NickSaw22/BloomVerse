import { inject, Injectable } from '@angular/core';
import { AccountService } from './account-service';
import { Observable, of } from 'rxjs';
@Injectable({
  providedIn: 'root',
})
export class InitService {
  private accountService = inject(AccountService);

  init() {
    const userJsonStr = localStorage.getItem('user');
    if (!userJsonStr) return of(null);
    const user = JSON.parse(userJsonStr);
    this.accountService.setCurrentUser(user);
    return of(null);
  }
}
