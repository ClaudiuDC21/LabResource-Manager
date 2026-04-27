import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { UserService } from '../../../core/services/user.service';
import { BorrowingService } from '../../../core/services/borrowing.service';
import { UserResponse } from '../../../core/models/user';
import { ActiveBorrowingResponse, UserBorrowingHistoryResponse } from '../../../core/models/borrowing';
import { UserRole, BorrowingStatus } from '../../../core/models/enums';

@Component({
  selector: 'app-user-details',
  standalone: true,
  imports: [CommonModule, ButtonModule, TableModule, TagModule],
  templateUrl: './user-details.html'
})
export class UserDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly userService = inject(UserService);
  private readonly borrowingService = inject(BorrowingService);

  user = signal<UserResponse | null>(null);
  activeBorrowings = signal<ActiveBorrowingResponse[]>([]);
  historyBorrowings = signal<UserBorrowingHistoryResponse[]>([]);
  isLoading = signal<boolean>(true);

  ngOnInit() {
    const userId = this.route.snapshot.paramMap.get('id');
    if (userId) {
      this.loadUserData(userId);
    } else {
      this.goBack();
    }
  }

  loadUserData(id: string) {
    this.isLoading.set(true);

    this.userService.getById(id).subscribe({
      next: (userData) => {
        this.user.set(userData);
      },
      error: () => this.goBack()
    });

    this.borrowingService.getActiveForUser(id).subscribe({
      next: (borrowingsData) => {
        this.activeBorrowings.set(borrowingsData);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });

    this.borrowingService.getUserHistory(id).subscribe({
      next: (historyData) => {
        this.historyBorrowings.set(historyData);
      }
    });
  }

  getRoleName(role: UserRole | string | number): string {
    const roleValue = typeof role === 'string' ? parseInt(role, 10) : role;
    return roleValue === UserRole.Teacher ? 'Teacher' : 'Student';
  }

  getRoleSeverity(role: UserRole | string | number): 'success' | 'info' {
    const roleValue = typeof role === 'string' ? parseInt(role, 10) : role;
    return roleValue === UserRole.Teacher ? 'info' : 'success';
  }

  getBorrowingSeverity(status: BorrowingStatus): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (status) {
      case BorrowingStatus.Active: return 'success';
      case BorrowingStatus.Pending: return 'info';
      case BorrowingStatus.Approved: return 'warn';
      case BorrowingStatus.Returned: return 'secondary';
      case BorrowingStatus.Rejected: return 'danger';
      default: return 'info';
    }
  }

  getBorrowingStatusName(status: BorrowingStatus): string {
    const names = {
      [BorrowingStatus.Pending]: 'Pending',
      [BorrowingStatus.Approved]: 'Approved',
      [BorrowingStatus.Active]: 'Active',
      [BorrowingStatus.Returned]: 'Returned',
      [BorrowingStatus.Rejected]: 'Rejected'
    };
    return names[status] || 'Unknown';
  }

  getTimelinessStatus(endDate: string, actualReturnDate?: string | null): string {
    const expected = new Date(endDate).getTime();
    const actual = actualReturnDate ? new Date(actualReturnDate).getTime() : Date.now();
    return actual > expected ? 'Exceeded' : 'On Time';
  }

  getTimelinessSeverity(endDate: string, actualReturnDate?: string | null): 'success' | 'danger' {
    const expected = new Date(endDate).getTime();
    const actual = actualReturnDate ? new Date(actualReturnDate).getTime() : Date.now();
    return actual > expected ? 'danger' : 'success';
  }

  isUserActive(userObj: UserResponse | null): boolean {
    if (!userObj) return false;
    return userObj.isActive === true || String(userObj.isActive).toLowerCase() === 'true';
  }

  goBack() {
    this.router.navigate(['/users']);
  }
}