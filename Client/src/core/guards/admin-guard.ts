import { CanActivateFn } from '@angular/router';
import { AccountService } from '../services/account-service';
import { ToastService } from '../services/toast-service';
import { inject } from '@angular/core';

export const adminGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const toastService = inject(ToastService);
  const user = accountService.currentUser();
  if (user && (user.roles.includes('Admin') || user.roles.includes('Moderator'))) {
    return true;
  } else {
    toastService.error('You do not have permission to access this page.');
    return false;
  }
};
