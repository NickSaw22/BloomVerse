import { Component, inject, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { User, UserRegister } from '../../types/user';
import { AccountService } from '../../core/services/account-service';

@Component({
  selector: 'app-register',
  imports: [FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private accountService = inject(AccountService);
  protected creds = {} as UserRegister;
  cancelRegister = output<boolean>();
  register() {
    this.accountService.register(this.creds).subscribe({
      next: response =>{
        this.accountService.currentUser.set(response);
        console.log('Registration successful:', response);
        this.cancelRegister.emit(false);
      },
      error: err =>{
        console.error('Registration failed:', err);
      }
    });
  }

  cancel() {
    this.creds = {} as UserRegister;
    this.cancelRegister.emit(false);
  }
}
