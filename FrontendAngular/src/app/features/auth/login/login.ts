import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { ButtonModule } from 'primeng/button';
import { FormsModule } from '@angular/forms';
import { BackendConfigService } from '../../../core/services/backend-config';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule, 
    RouterLink, 
    InputTextModule, 
    CheckboxModule, 
    ButtonModule, 
    FormsModule
  ],
  templateUrl: './login.html'
})
export class LoginComponent {
  backendService = inject(BackendConfigService);
  private router = inject(Router);

  handleLogin() {
    this.router.navigate(['/dashboard']);
  }
}