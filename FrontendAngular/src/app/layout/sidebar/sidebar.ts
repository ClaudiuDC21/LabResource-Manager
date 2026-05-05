import { Component, OnInit, inject, signal, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { BadgeModule } from 'primeng/badge';
import { LayoutService } from '../services/layout.service';
import { AuthService } from '../../core/services/auth.service';
import { BorrowingService } from '../../core/services/borrowing.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, BadgeModule],
  templateUrl: './sidebar.html'
})
export class SidebarComponent implements OnInit {
  readonly layoutService = inject(LayoutService);
  readonly authService = inject(AuthService);
  private readonly borrowingService = inject(BorrowingService);
  private readonly destroyRef = inject(DestroyRef);

  pendingApprovalsCount = signal<number>(0);

  ngOnInit() {
    this.loadPendingCount();

    this.borrowingService.pendingCountUpdated$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.loadPendingCount();
      });
  }

  loadPendingCount() {
    const user = this.authService.currentUser();
    if (user && (user.role === 'Teacher' || user.role === 2)) {
      this.borrowingService.getPendingForTeacher().subscribe({
        next: (data) => this.pendingApprovalsCount.set(data.length),
        error: () => this.pendingApprovalsCount.set(0)
      });
    }
  }
}