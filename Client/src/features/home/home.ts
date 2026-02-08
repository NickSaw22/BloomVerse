import { Component, Input, signal } from '@angular/core';
import { Register } from "../register/register";
import { required } from '@angular/forms/signals';
import { User } from '../../types/user';

@Component({
  selector: 'app-home',
  imports: [Register],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  protected registerMode = signal(false);

  showRegisterMode(val: boolean) {
    this.registerMode.set(val);
  }
}
