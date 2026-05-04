import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { ProgressBarModule } from 'primeng/progressbar';
import { TagModule } from 'primeng/tag';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

import { AuthService } from '../../../core/services/auth.service';
import { BorrowingService } from '../../../core/services/borrowing.service';
import { ActiveBorrowingResponse, ReturnAssetRequest, UserBorrowingHistoryResponse } from '../../../core/models/borrowing';
import { BorrowingStatus } from '../../../core/models/enums';

export interface MappedBorrowing extends ActiveBorrowingResponse {
  progressValue: number;
  timeLeftLabel: string;
  isExceeded: boolean;
}

@Component({
  selector: 'app-my-borrowings',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    TableModule, 
    ButtonModule, 
    DialogModule, 
    CheckboxModule,
    ProgressBarModule,
    TagModule,
    TextareaModule,
    ToastModule
  ],
  providers: [MessageService],
  templateUrl: './my-borrowings.html'
})
export class MyBorrowingsComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly borrowingService = inject(BorrowingService);
  private readonly messageService = inject(MessageService);

  // Facem enum-ul disponibil în HTML
  public readonly BorrowingStatus = BorrowingStatus;

  activeBorrowings = signal<MappedBorrowing[]>([]);
  requestHistory = signal<UserBorrowingHistoryResponse[]>([]);
  
  isLoadingActive = signal<boolean>(true);
  isLoadingHistory = signal<boolean>(true);

  returnDialog = false;
  selectedAssetToReturn: MappedBorrowing | null = null;
  returnForm: ReturnAssetRequest = {
    labAssetId: '',
    remarks: '',
    isDefective: false
  };

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    const userId = this.authService.currentUser()?.id;
    if (!userId) return;

    // Încarcă împrumuturile Active și Approved
    this.isLoadingActive.set(true);
    this.borrowingService.getActiveForUser(userId).subscribe({
      next: (data) => {
        const mappedData = data.map(b => this.calculateProgress(b));
        this.activeBorrowings.set(mappedData);
        this.isLoadingActive.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load active borrowings.' });
        this.isLoadingActive.set(false);
      }
    });

    // Încarcă tot istoricul (inclusiv Pending, Rejected, Returned)
    this.isLoadingHistory.set(true);
    this.borrowingService.getUserHistory(userId).subscribe({
      next: (data) => {
        this.requestHistory.set(data);
        this.isLoadingHistory.set(false);
      },
      error: () => this.isLoadingHistory.set(false)
    });
  }

  private calculateProgress(borrowing: ActiveBorrowingResponse): MappedBorrowing {
    const start = new Date(borrowing.requestedStartDate).getTime();
    const end = new Date(borrowing.requestedEndDate).getTime();
    const now = Date.now();

    // Dacă încă nu a fost preluat (Approved, dar nu Active), progresul e 0
    if (borrowing.status === BorrowingStatus.Approved) {
      return {
        ...borrowing,
        progressValue: 0,
        timeLeftLabel: 'Waiting for pick-up',
        isExceeded: false
      };
    }

    const totalDuration = end - start;
    const elapsed = now - start;

    let progress = 0;
    if (totalDuration > 0) {
        progress = Math.round((elapsed / totalDuration) * 100);
    }
    
    progress = Math.max(0, Math.min(100, progress));
    
    const isExceeded = now > end;
    let timeLeftLabel = '';

    if (isExceeded) {
      timeLeftLabel = 'Overdue';
    } else if (now < start) {
      timeLeftLabel = 'Not started yet';
    } else {
      const msLeft = end - now;
      const hoursLeft = Math.floor(msLeft / (1000 * 60 * 60));
      const daysLeft = Math.floor(hoursLeft / 24);

      if (daysLeft > 0) {
        timeLeftLabel = `${daysLeft}d ${hoursLeft % 24}h left`;
      } else {
        timeLeftLabel = `${hoursLeft}h left`;
      }
    }

    return {
      ...borrowing,
      progressValue: progress,
      timeLeftLabel: timeLeftLabel,
      isExceeded: isExceeded
    };
  }

  getStatusName(status: BorrowingStatus): string {
    switch (status) {
      case BorrowingStatus.Pending: return 'Pending Approval';
      case BorrowingStatus.Approved: return 'Approved (Awaiting Pickup)';
      case BorrowingStatus.Active: return 'Active';
      case BorrowingStatus.Returned: return 'Returned';
      case BorrowingStatus.Rejected: return 'Rejected';
      default: return 'Unknown';
    }
  }

  getStatusSeverity(status: BorrowingStatus): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (status) {
      case BorrowingStatus.Pending: return 'info';
      case BorrowingStatus.Approved: return 'warn';
      case BorrowingStatus.Active: return 'success';
      case BorrowingStatus.Returned: return 'secondary';
      case BorrowingStatus.Rejected: return 'danger';
      default: return 'info';
    }
  }

  pickUpAsset(borrowing: MappedBorrowing) {
    this.borrowingService.pickUpAsset(borrowing.borrowingRecordId).subscribe({
      next: () => {
        this.messageService.add({ 
          severity: 'success', 
          summary: 'Picked Up', 
          detail: `You have successfully picked up ${borrowing.assetName}.` 
        });
        this.loadData();
      },
      error: (err) => {
        this.messageService.add({ 
          severity: 'error', 
          summary: 'Error', 
          detail: err.error?.Error || 'Failed to pick up the asset.' 
        });
      }
    });
  }

  openReturnDialog(borrowing: MappedBorrowing) {
    this.selectedAssetToReturn = borrowing;
    this.returnForm = {
      labAssetId: borrowing.labAssetId,
      remarks: '',
      isDefective: false
    };
    this.returnDialog = true;
  }

  hideDialog() {
    this.returnDialog = false;
    this.selectedAssetToReturn = null;
  }

  submitReturn() {
    if (!this.selectedAssetToReturn) return;

    const borrowingId = this.selectedAssetToReturn.borrowingRecordId;

    this.borrowingService.returnAsset(borrowingId, this.returnForm).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Asset returned successfully.' });
        this.loadData(); 
        this.hideDialog();
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.Error || 'Error returning asset.' });
      }
    });
  }
}