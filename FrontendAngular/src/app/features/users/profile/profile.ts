import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { InputTextModule } from 'primeng/inputtext';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ConfirmationService, MessageService } from 'primeng/api';
import { AuthService } from '../../../core/services/auth.service';
import { UserService } from '../../../core/services/user.service';
import { BorrowingService } from '../../../core/services/borrowing.service';
import { UpdatePasswordRequest, UpdateUserRequest, UserResponse } from '../../../core/models/user';
import { ActiveBorrowingResponse, UserBorrowingHistoryResponse } from '../../../core/models/borrowing';
import { UIHelpers } from '../../../core/models/ui-helpers';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    ButtonModule, 
    PasswordModule, 
    InputTextModule, 
    DividerModule, 
    MessageModule,
    ConfirmDialogModule,
    ToastModule,
    TableModule,
    TagModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './profile.html'
})
export class ProfileComponent implements OnInit {
  readonly authService = inject(AuthService);
  private readonly userService = inject(UserService);
  private readonly borrowingService = inject(BorrowingService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);

  public readonly UIHelpers = UIHelpers;

  isEditingProfile = signal<boolean>(false);
  isLoading = signal<boolean>(false);
  userFromApi: UserResponse | null = null;

  activeBorrowings = signal<ActiveBorrowingResponse[]>([]);
  borrowingHistory = signal<UserBorrowingHistoryResponse[]>([]);

  profileForm: UpdateUserRequest = {
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
    this.loadBorrowings();
  }

  loadUserData() {
    const userId = this.authService.currentUser()?.id;
    if (!userId) return;

    this.isLoading.set(true);
    this.userService.getById(userId).subscribe({
      next: (user) => {
        this.userFromApi = user;
        this.resetForm();
        this.isLoading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load profile data.' });
        this.isLoading.set(false);
      }
    });
  }

  loadBorrowings() {
    const userId = this.authService.currentUser()?.id;
    if (!userId) return;

    this.borrowingService.getActiveForUser(userId).subscribe({
      next: (data) => this.activeBorrowings.set(data)
    });

    this.borrowingService.getUserHistory(userId).subscribe({
      next: (data) => this.borrowingHistory.set(data)
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

    this.userService.update(userId, this.profileForm).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Profile updated successfully!' });
        this.isEditingProfile.set(false);
        this.loadUserData(); 
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.detail || 'Update failed.' });
      }
    });
  }

  changePassword() {
    const userId = this.authService.currentUser()?.id;
    if (!userId) return;

    if (this.passwords.newPassword !== this.passwords.confirmPassword) {
      this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'The new passwords do not match!' });
      return;
    }

    const payload: UpdatePasswordRequest = {
      currentPassword: this.passwords.currentPassword,
      newPassword: this.passwords.newPassword
    };

    this.userService.updatePassword(userId, payload).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Password changed successfully!' });
        this.passwords = { currentPassword: '', newPassword: '', confirmPassword: '' };
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Check your current password.' });
      }
    });
  }

  deactivateAccount() {
    const userId = this.authService.currentUser()?.id;
    if (!userId) return;

    this.confirmationService.confirm({
      message: 'Are you sure you want to deactivate your account?',
      header: 'Confirm Deactivation',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Deactivate', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', text: true },
      accept: () => {
        this.userService.deactivate(userId).subscribe({
          next: () => {
            this.authService.logout();
          }
        });
      }
    });
  }
}