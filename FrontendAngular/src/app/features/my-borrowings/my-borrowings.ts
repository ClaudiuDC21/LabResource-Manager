import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { AuthService } from '../../core/services/auth';
import { BorrowingService } from '../../core/services/borrowing';
import { ActiveBorrowingResponse, ReturnAssetRequest } from '../../core/models/borrowing';

@Component({
  selector: 'app-my-borrowings',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    TableModule, 
    ButtonModule, 
    DialogModule, 
    CheckboxModule
  ],
  templateUrl: './my-borrowings.html'
})
export class MyBorrowingsComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly borrowingService = inject(BorrowingService);

  activeBorrowings = signal<ActiveBorrowingResponse[]>([]);
  isLoading = signal<boolean>(true);

  returnDialog = false;
  selectedAssetToReturn: ActiveBorrowingResponse | null = null;
  returnForm: ReturnAssetRequest = {
    labAssetId: '',
    remarks: '',
    isDefective: false
  };

  ngOnInit() {
    this.loadActiveBorrowings();
  }

  loadActiveBorrowings() {
    const userId = this.authService.currentUser()?.id;
    if (!userId) return;

    this.isLoading.set(true);
    this.borrowingService.getActiveForUser(userId).subscribe({
      next: (data) => {
        this.activeBorrowings.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.isLoading.set(false);
      }
    });
  }

  openReturnDialog(borrowing: ActiveBorrowingResponse) {
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

    this.borrowingService.returnAsset(this.returnForm).subscribe({
      next: () => {
        this.loadActiveBorrowings();
        this.hideDialog();
      },
      error: (err) => {
        console.error(err);
        alert('Error returning asset.');
      }
    });
  }
}