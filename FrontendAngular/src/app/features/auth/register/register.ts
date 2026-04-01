import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { FormsModule } from '@angular/forms';
import { BackendConfigService } from '../../../core/services/backend-config';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule, 
    RouterLink, 
    InputTextModule, 
    ButtonModule, 
    FormsModule
  ],
  templateUrl: './register.html'
})
export class RegisterComponent {
  backendService = inject(BackendConfigService);
  private router = inject(Router);

  handleRegister() {
    this.router.navigate(['/login']);
  }
}