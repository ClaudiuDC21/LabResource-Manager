import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';
import { UserService } from '../../core/services/user.service';
import { UserResponse } from '../../core/models/user';
import { Router } from '@angular/router';
import { UserRole } from '../../core/models/enums';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    TagModule,
    ConfirmDialogModule,
    ToastModule,
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './users.html',
})
export class UsersComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly messageService = inject(MessageService);
   private readonly router = inject(Router);

  users = signal<UserResponse[]>([]);
  isLoading = signal<boolean>(true);

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.isLoading.set(true);
    this.userService.getAllActive().subscribe({
      next: (data) => {
        this.users.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load users.',
        });
        this.isLoading.set(false);
      },
    });
  }

  getRoleSeverity(role: UserRole | string | number): 'success' | 'info' {
    // În funcție de cum face maparea JSON-ul (număr sau string)
    const roleValue = typeof role === 'string' ? parseInt(role, 10) : role;
    return roleValue === UserRole.Teacher ? 'info' : 'success';
  }

  getRoleName(role: UserRole | string | number): string {
    const roleValue = typeof role === 'string' ? parseInt(role, 10) : role;
    return roleValue === UserRole.Teacher ? 'Teacher' : 'Student';
  }

  viewUserDetails(user: UserResponse) {
    this.router.navigate(['/users', user.id]);
  }

  deactivateUser(user: UserResponse, event: Event) {
    event.stopPropagation();

    this.confirmationService.confirm({
      message: `Are you sure you want to deactivate ${user.fullName}? This will instantly revoke their access to the platform.`,
      header: 'Confirm Deactivation',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text p-button-secondary',
      accept: () => {
        this.userService.deactivate(user.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Deactivated',
              detail: 'User account has been deactivated.',
            });
            this.loadUsers();
          },
          error: (err) => {
            console.error(err);
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to deactivate user.',
            });
          },
        });
      },
    });
  }
}
