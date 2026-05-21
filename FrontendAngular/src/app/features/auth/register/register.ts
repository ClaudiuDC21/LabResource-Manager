import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { BackendConfigService } from '../../../core/services/backend-config.service';
import { AuthService } from '../../../core/services/auth.service';

export const passwordMatchValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('password')?.value;
  const confirmControl = control.get('confirm');
  
  if (!confirmControl) {
    return null;
  }

  if (password && confirmControl.value && password !== confirmControl.value) {
    confirmControl.setErrors({ ...confirmControl.errors, passwordMismatch: true });
    return { passwordMismatch: true };
  }

  if (confirmControl.errors) {
    const { passwordMismatch, ...remainingErrors } = confirmControl.errors;
    confirmControl.setErrors(Object.keys(remainingErrors).length ? remainingErrors : null);
  }
  
  return null;
};

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
  readonly backendService = inject(BackendConfigService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  isLoading = false;
  errorMessage = '';

  registerForm: FormGroup = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    matriculationNumber: [''], 
    password: ['', [
      Validators.required, 
      Validators.minLength(8),
      Validators.pattern(/(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9])/)
    ]],
    confirm: ['', Validators.required]
  }, { validators: passwordMatchValidator });

  handleRegister() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const { fullName, email, matriculationNumber, password } = this.registerForm.value;

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
        this.errorMessage = err.error?.detail || 'Registration failed. Email might already be in use.';
        console.error(err);
      }
    });
  }
}