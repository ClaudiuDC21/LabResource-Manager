import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';
import { AuthService } from '../../core/services/auth';
import { LabAssetService } from '../../core/services/lab-asset';
import { BorrowingService } from '../../core/services/borrowing';
import { LabAsset, CreateLabAssetRequest, AssetStatus } from '../../core/models/lab-asset';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    TableModule, 
    ButtonModule, 
    InputTextModule, 
    DialogModule, 
    TagModule,
    ConfirmDialogModule,
    ToastModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './dashboard.html'
})
export class DashboardComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly assetService = inject(LabAssetService);
  private readonly borrowingService = inject(BorrowingService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly messageService = inject(MessageService);

  assets = signal<LabAsset[]>([]);
  isLoading = signal<boolean>(true);

  assetDialog = false;
  isEditing = false;
  currentAssetId: string | null = null;
  
  assetForm: CreateLabAssetRequest = {
    name: '',
    serialNumber: ''
  };

  ngOnInit() {
    this.loadAssets();
  }

  loadAssets() {
    this.isLoading.set(true);
    this.assetService.getAllActive().subscribe({
      next: (data) => {
        this.assets.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.isLoading.set(false);
      }
    });
  }

  get isTeacher(): boolean {
    return this.authService.currentUser()?.role === 'Teacher';
  }

  getStatusSeverity(status: AssetStatus): 'success' | 'warn' | 'danger' {
    switch (status) {
      case AssetStatus.Available: return 'success';
      case AssetStatus.Borrowed: return 'warn';
      case AssetStatus.Defective: return 'danger';
      default: return 'success';
    }
  }

  getStatusName(status: AssetStatus): string {
    switch (status) {
      case AssetStatus.Available: return 'Available';
      case AssetStatus.Borrowed: return 'Borrowed';
      case AssetStatus.Defective: return 'Defective';
      default: return 'Unknown';
    }
  }

  openNew() {
    this.assetForm = { name: '', serialNumber: '' };
    this.isEditing = false;
    this.currentAssetId = null;
    this.assetDialog = true;
  }

  editAsset(asset: LabAsset) {
    this.assetForm = { name: asset.name, serialNumber: asset.serialNumber };
    this.currentAssetId = asset.id;
    this.isEditing = true;
    this.assetDialog = true;
  }

  hideDialog() {
    this.assetDialog = false;
  }

  saveAsset() {
    if (!this.validateForm()) return;

    if (this.isEditing && this.currentAssetId) {
      this.assetService.update(this.currentAssetId, this.assetForm).subscribe({
        next: () => {
          this.loadAssets();
          this.hideDialog();
          this.messageService.add({ severity: 'success', summary: 'Updated', detail: 'Asset updated successfully.' });
        },
        error: (err: any) => this.handleError(err) 
      });
    } else {
      this.assetService.create(this.assetForm).subscribe({
        next: () => {
          this.loadAssets();
          this.hideDialog();
          this.messageService.add({ severity: 'success', summary: 'Created', detail: 'Asset created successfully.' });
        },
        error: (err: any) => this.handleError(err) 
      });
    }
  }

  private validateForm(): boolean {
    return !!this.assetForm.name && this.assetForm.name.trim().length > 0;
  }

  private handleError(err: any) {
    console.error(err);
    this.messageService.add({ severity: 'error', summary: 'Error', detail: 'An error occurred. Please try again.' });
  }

  deleteAsset(asset: LabAsset) {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete ${asset.name}?`,
      header: 'Confirm Deletion',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text p-button-secondary',
      accept: () => {
        this.assetService.deactivate(asset.id).subscribe({
          next: () => {
            this.loadAssets();
            this.messageService.add({ severity: 'success', summary: 'Deleted', detail: 'Asset successfully removed.' });
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete asset.' })
        });
      }
    });
  }

  borrowAsset(asset: LabAsset) {
    const user = this.authService.currentUser();
    if (!user) return;

    const request = {
      userId: user.id,
      labAssetId: asset.id
    };

    this.confirmationService.confirm({
      message: `Do you want to borrow the ${asset.name}?`,
      header: 'Confirm Borrow',
      icon: 'pi pi-info-circle',
      acceptButtonStyleClass: 'bg-logo-green border-none hover:bg-green-700',
      rejectButtonStyleClass: 'p-button-text p-button-secondary',
      accept: () => {
        this.borrowingService.borrow(request).subscribe({
          next: (response) => {
            this.loadAssets();
            this.messageService.add({ severity: 'success', summary: 'Success', detail: `Successfully borrowed ${response.assetName}!` });
          },
          error: (err) => {
            console.error(err);
            this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.Error || 'Failed to borrow asset.' });
          }
        });
      }
    });
  }
}