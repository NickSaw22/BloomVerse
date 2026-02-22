import { inject, Injectable } from '@angular/core';
import { AccountService } from './account-service';
import { catchError, Observable, of, tap } from 'rxjs';
import { LikesService } from './likes-service';
@Injectable({
  providedIn: 'root',
})
export class InitService {
  private accountService = inject(AccountService);
  private likesService = inject(LikesService);

  init() {
    return this.accountService.refereshToken().pipe(
      tap((user) => {
        if (user) {
          this.accountService.setCurrentUser(user);
          this.accountService.startTokenRefreshInterval();
        }
      }),
      catchError((err) => {
        console.error('Error refreshing token on app init:', err);
        return of(null);
      })
    );
  }
}
