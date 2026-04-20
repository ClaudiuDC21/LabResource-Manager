import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { InputTextModule } from 'primeng/inputtext';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../core/services/auth';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, PasswordModule, InputTextModule, DividerModule, MessageModule],
  templateUrl: './profile.html'
})
export class ProfileComponent implements OnInit {
  authService = inject(AuthService);
  private http = inject(HttpClient);

  isEditingProfile = signal<boolean>(false);
  isLoading = signal<boolean>(false);

  userFromApi: any = null;

  profileForm = {
    fullName: '',
    matriculationNumber: ''
  };

  passwords = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  ngOnInit() {
    this.loadUserData();
  }

  loadUserData() {
    const userId = this.authService.currentUser()?.id;
    if (!userId) return;

    this.isLoading.set(true);
    this.http.get<any>(`/api/users/${userId}`).subscribe({
      next: (user) => {
        this.userFromApi = user;
        this.resetForm();
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading data', err);
        this.isLoading.set(false);
      }
    });
  }

  startEditing() {
    this.resetForm();
    this.isEditingProfile.set(true);
  }

  cancelEditing() {
    this.resetForm();
    this.isEditingProfile.set(false);
  }

  private resetForm() {
    if (this.userFromApi) {
      this.profileForm.fullName = this.userFromApi.fullName || '';
      this.profileForm.matriculationNumber = this.userFromApi.matriculationNumber || '';
    }
  }

  updateProfile() {
    const userId = this.authService.currentUser()?.id;
    if (!userId || !this.userFromApi) return;

    const payload = {
      id: userId,
      fullName: this.profileForm.fullName,
      matriculationNumber: this.profileForm.matriculationNumber
    };

    this.http.put(`/api/users/${userId}`, payload).subscribe({
      next: () => {
        alert('Profile updated successfully!');
        this.isEditingProfile.set(false);
        this.loadUserData(); 
      },
      error: (err) => {
        alert('An error occurred during the update. Please check your data.');
        console.error(err);
      }
    });
  }

  changePassword() {
    const userId = this.authService.currentUser()?.id;
    if (!userId) return;

    if (this.passwords.newPassword !== this.passwords.confirmPassword) {
      alert('The new passwords do not match!');
      return;
    }

    const payload = {
      id: userId,
      currentPassword: this.passwords.currentPassword,
      newPassword: this.passwords.newPassword
    };

    this.http.put(`/api/users/${userId}/password`, payload).subscribe({
      next: () => {
        alert('Password changed successfully!');
        this.passwords = { currentPassword: '', newPassword: '', confirmPassword: '' };
      },
      error: (err) => alert('An error occurred. Please check your current password.')
    });
  }

  deactivateAccount() {
    const userId = this.authService.currentUser()?.id;
    if (!userId) return;

    if (confirm('Are you sure you want to deactivate your account? This action is irreversible.')) {
      this.http.delete(`/api/users/${userId}`).subscribe({
        next: () => {
          alert('Account deactivated. You will be logged out.');
          this.authService.logout();
        },
        error: (err) => alert('Error deactivating account.')
      });
    }
  }
}