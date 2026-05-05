import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

import { AuthService } from '../../../core/services/auth.service';
import { BorrowingService } from '../../../core/services/borrowing.service';
import { ActiveBorrowingResponse, ReviewBorrowingRequest } from '../../../core/models/borrowing';

@Component({
  selector: 'app-approvals',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    TableModule, 
    ButtonModule, 
    DialogModule, 
    TextareaModule,
    ToastModule
  ],
  providers: [MessageService],
  templateUrl: './approvals.html'
})
export class ApprovalsComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly borrowingService = inject(BorrowingService);
  private readonly messageService = inject(MessageService);

  pendingRequests = signal<ActiveBorrowingResponse[]>([]);
  isLoading = signal<boolean>(true);

  reviewDialog = false;
  isApproving = false;
  selectedRequestId: string | null = null;
  
  reviewForm: ReviewBorrowingRequest = {
    isApproved: false,
    teacherNotes: ''
  };

  ngOnInit() {
    this.loadPendingRequests();
  }

  loadPendingRequests() {
    const user = this.authService.currentUser();
    if (!user) return;

    this.isLoading.set(true);

    this.borrowingService.getPendingForTeacher().subscribe({
      next: (data) => {
        this.pendingRequests.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Could not load pending requests.' });
        this.isLoading.set(false);
      }
    });
  }

  openReviewDialog(request: ActiveBorrowingResponse, approve: boolean) {
    this.selectedRequestId = request.borrowingRecordId;
    this.isApproving = approve;
    this.reviewForm = {
      isApproved: approve,
      teacherNotes: ''
    };
    this.reviewDialog = true;
  }

  hideDialog() {
    this.reviewDialog = false;
    this.selectedRequestId = null;
  }

  submitReview() {
    if (!this.selectedRequestId) return;

    this.borrowingService.reviewRequest(this.selectedRequestId, this.reviewForm).subscribe({
      next: () => {
        const action = this.isApproving ? 'approved' : 'rejected';
        this.messageService.add({ severity: 'success', summary: 'Success', detail: `Request ${action} successfully.` });
        
        this.loadPendingRequests();
        this.hideDialog();

        this.borrowingService.notifyPendingCountChanged(); 
      },
      error: (err) => {
        const detail = err.error?.detail || 'Failed to process the request.';
        this.messageService.add({ severity: 'error', summary: 'Error', detail: detail });
      }
    });
  }
}