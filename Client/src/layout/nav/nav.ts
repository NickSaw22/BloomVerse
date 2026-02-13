import { Component, inject, OnInit, signal } from '@angular/core';
import { AccountService } from '../../core/services/account-service';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from "@angular/router";
import { ToastService } from '../../core/services/toast-service';
import { themes } from '../theme';
import { BusyService } from '../../core/services/busy-service';
@Component({
  selector: 'app-nav',
  imports: [FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav implements OnInit {
  protected accountService = inject(AccountService);
  private toastService = inject(ToastService)
  private router = inject(Router);
  protected busyService = inject(BusyService);

  protected creds: any = {}
  protected isLoggedIn = signal(false);
  protected selectedTheme = signal<string>(localStorage.getItem('theme') || 'light')
  protected themes = themes;
    
  ngOnInit() {
    document.documentElement.setAttribute('data-theme', this.selectedTheme());
  }

  handleSelectedTheme(theme: string) {
    this.selectedTheme.set(theme);
    localStorage.setItem('theme', theme);
    document.documentElement.setAttribute('data-theme', theme);
    const elem = document.activeElement as HTMLDivElement;
    if(elem){
      elem.blur();
    }
  }

  userLogin() {
    console.log('Login button clicked with credentials:', this.creds);
    this.accountService.login(this.creds).subscribe({
      next: response =>{
        console.log('Login successful:', response);
        this.router.navigateByUrl('/members');
        this.isLoggedIn.set(true);
        this.toastService.success('Login successful!', 3000);
      },
      error: err =>{
        console.error('Login failed:', err);
        this.isLoggedIn.set(false);
        this.toastService.error(err.error || 'Login failed. Please try again.', 5000);
      }
    });
  }

  userLogout() {
    console.log('Logout button clicked');
    this.router.navigateByUrl('/');
    this.creds = {};
    this.accountService.logout();
    this.isLoggedIn.set(false);
  }
}
