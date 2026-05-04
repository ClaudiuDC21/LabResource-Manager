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
import { UIHelpers } from '../../../core/models/ui-helpers';

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

  public readonly BorrowingStatus = BorrowingStatus;
  public readonly UIHelpers = UIHelpers;

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

    this.isLoadingActive.set(true);
    this.borrowingService.getActiveForUser(userId).subscribe({
      next: (data) => {
        const mappedData = data.map(b => ({
          ...b,
          ...UIHelpers.calculateProgress(b.requestedStartDate, b.requestedEndDate, b.status)
        }));
        this.activeBorrowings.set(mappedData);
        this.isLoadingActive.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load active borrowings.' });
        this.isLoadingActive.set(false);
      }
    });

    this.isLoadingHistory.set(true);
    this.borrowingService.getUserHistory(userId).subscribe({
      next: (data) => {
        this.requestHistory.set(data);
        this.isLoadingHistory.set(false);
      },
      error: () => this.isLoadingHistory.set(false)
    });
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