import { Component, inject, signal } from '@angular/core';
import { AccountService } from '../../core/services/account-service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-nav',
  imports: [FormsModule],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
  protected accountService = inject(AccountService);
  protected creds: any = {}
  protected isLoggedIn = signal(false);

  userLogin() {
    console.log('Login button clicked with credentials:', this.creds);
    this.accountService.login(this.creds).subscribe({
      next: response =>{
        console.log('Login successful:', response);
        this.isLoggedIn.set(true);
      },
      error: err =>{
        console.error('Login failed:', err);
        this.isLoggedIn.set(false);
      }
    });
  }

  userLogout() {
    console.log('Logout button clicked');
    this.creds = {};
    this.accountService.logout();
    this.isLoggedIn.set(false);
  }
}
