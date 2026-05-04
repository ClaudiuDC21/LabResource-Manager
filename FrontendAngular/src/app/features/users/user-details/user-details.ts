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
import { UIHelpers } from '../../../core/models/ui-helpers';

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

  public readonly UIHelpers = UIHelpers;

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

  goBack() {
    this.router.navigate(['/users']);
  }
}