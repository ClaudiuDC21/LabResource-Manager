import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { BackendConfigService } from '../../../core/services/backend-config';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule, 
    RouterLink, 
    InputTextModule, 
    ReactiveFormsModule
  ],
  templateUrl: './register.html'
})
export class RegisterComponent {
  private readonly backendService = inject(BackendConfigService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  isLoading = false;
  errorMessage = '';

  registerForm: FormGroup = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    matriculationNumber: [''], // Câmp opțional
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirm: ['', Validators.required]
  });

  handleRegister() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const { fullName, email, matriculationNumber, password, confirm } = this.registerForm.value;

    if (password !== confirm) {
      this.errorMessage = 'Passwords do not match!';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const payload = {
      fullName,
      email,
      password,
      matriculationNumber: matriculationNumber || null
    };

    this.authService.register(payload).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.isLoading = false;
        // Prindem eroarea din C#: new { Error = ex.Message }
        this.errorMessage = err.error?.error || err.error?.Error || 'Registration failed. Email might already be in use.';
        console.error('Register error:', err);
      }
    });
  }
}