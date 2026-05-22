import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { SelectModule } from 'primeng/select';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { CheckboxModule } from 'primeng/checkbox';
import { ConfirmationService, MessageService } from 'primeng/api';

import { AuthService } from '../../core/services/auth.service';
import { LabAssetService } from '../../core/services/lab-asset.service';
import { UserService } from '../../core/services/user.service';
import { LabAsset, CreateLabAssetRequest } from '../../core/models/lab-asset';
import { UserResponse } from '../../core/models/user';
import { AssetStatus } from '../../core/models/enums';

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
    ToastModule,
    SelectModule,
    IconFieldModule,
    InputIconModule,
    CheckboxModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './dashboard.html'
})
export class DashboardComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly assetService = inject(LabAssetService);
  private readonly userService = inject(UserService);
  private readonly router = inject(Router);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly messageService = inject(MessageService);

  assets = signal<LabAsset[]>([]);
  teachers = signal<UserResponse[]>([]);
  isLoading = signal<boolean>(true);

  assetDialog = false;
  isEditing = false;
  currentAssetId: string | null = null;

  // Modificat din CreateLabAssetRequest in any pentru a permite adaugarea locala a isDefective
  assetForm: any = {
    name: '',
    serialNumber: '',
    location: '',
    assignedTeacherId: null,
    isDefective: false
  };

  ngOnInit() {
    this.loadAssets();
    if (this.isTeacher) {
       this.loadTeachers();
    }
  }

  loadAssets() {
    this.isLoading.set(true);
    this.assetService.getAllActive().subscribe({
      next: (data) => {
        this.assets.set(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  loadTeachers() {
    this.userService.getAllActive().subscribe({
      next: (users) => {
        const mappedTeachers = users
          .filter(u => u.role === 2 || u.role.toString() === 'Teacher')
          .map(u => ({ ...u, id: u.id.toLowerCase() }));
        this.teachers.set(mappedTeachers);
      },
      error: (err) => console.error('Error loading teachers:', err)
    });
  }

  get isTeacher(): boolean {
    const role = this.authService.currentUser()?.role;
    return role === 'Teacher' || role === 2;
  }

  getStatusSeverity(status: AssetStatus): 'success' | 'info' | 'warn' | 'danger' {
    switch (status) {
      case AssetStatus.Available: return 'success';
      case AssetStatus.PendingApproval: return 'info';
      case AssetStatus.Borrowed: return 'warn';
      case AssetStatus.Defective: return 'danger';
      default: return 'success';
    }
  }

  getStatusName(status: AssetStatus): string {
    switch (status) {
      case AssetStatus.Available: return 'Available';
      case AssetStatus.PendingApproval: return 'Pending Approval';
      case AssetStatus.Borrowed: return 'Borrowed';
      case AssetStatus.Defective: return 'Defective';
      default: return 'Unknown';
    }
  }

  getBorrowButtonLabel(status: AssetStatus): string {
    return status === AssetStatus.Available ? 'Borrow' : 'Schedule';
  }

  getBorrowButtonIcon(status: AssetStatus): string {
    return status === AssetStatus.Available ? 'pi pi-calendar-plus' : 'pi pi-calendar-clock';
  }

  getBorrowButtonSeverity(status: AssetStatus): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    return status === AssetStatus.Available ? 'success' : 'info';
  }

  canBorrow(status: AssetStatus): boolean {
    return status !== AssetStatus.Defective;
  }

  openNew() {
    if (this.teachers().length === 0 && this.isTeacher) {
       this.loadTeachers();
    }
    
    this.assetForm = {
      name: '',
      serialNumber: '',
      location: '',
      assignedTeacherId: null,
      isDefective: false
    };
    this.isEditing = false;
    this.currentAssetId = null;
    this.assetDialog = true;
  }

  editAsset(asset: LabAsset) {
    if (this.teachers().length === 0 && this.isTeacher) {
       this.loadTeachers();
    }

    this.assetForm = {
      name: asset.name,
      serialNumber: asset.serialNumber || '',
      location: asset.location || '',
      assignedTeacherId: asset.assignedTeacherId ? asset.assignedTeacherId.toLowerCase() : null,
      isDefective: asset.status === AssetStatus.Defective
    };
    this.currentAssetId = asset.id;
    this.isEditing = true;
    this.assetDialog = true;
  }

  hideDialog() {
    this.assetDialog = false;
  }

  saveAsset() {
    if (!this.assetForm.name?.trim()) return;

    const payload: any = {
      ...this.assetForm,
      serialNumber: this.assetForm.serialNumber?.trim() || null,
      location: this.assetForm.location?.trim() || null
    };

    if (this.isEditing && this.currentAssetId) {
      const updatePayload = {
        ...payload,
        isActive: true
      };

      this.assetService.update(this.currentAssetId, updatePayload).subscribe({
        next: () => {
          this.loadAssets();
          this.hideDialog();
          this.messageService.add({ severity: 'success', summary: 'Updated', detail: 'Asset updated successfully.' });
        },
        error: (err) => this.handleError(err)
      });
    } else {
      this.assetService.create(payload).subscribe({
        next: () => {
          this.loadAssets();
          this.hideDialog();
          this.messageService.add({ severity: 'success', summary: 'Created', detail: 'Asset created successfully.' });
        },
        error: (err) => this.handleError(err)
      });
    }
  }

  private handleError(err: any) {
    const detail = err.error?.detail || 'Action failed. Check console for details.';
    this.messageService.add({ severity: 'error', summary: 'Error', detail: detail });
  }

  deleteAsset(asset: LabAsset) {
    this.confirmationService.confirm({
      message: `Are you sure you want to deactivate ${asset.name}?`,
      header: 'Confirm Deactivation',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Deactivate', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', text: true },
      accept: () => {
        this.assetService.deactivate(asset.id).subscribe({
          next: () => {
            this.loadAssets();
            this.messageService.add({ severity: 'success', summary: 'Removed', detail: 'Asset deactivated.' });
          },
          error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to deactivate asset.' })
        });
      }
    });
  }

  viewAssetDetails(assetId: string) {
    this.router.navigate(['/dashboard/asset', assetId]);
  }

  borrowAsset(asset: LabAsset) {
    this.router.navigate(['/dashboard', asset.id, 'request']);
  }
}