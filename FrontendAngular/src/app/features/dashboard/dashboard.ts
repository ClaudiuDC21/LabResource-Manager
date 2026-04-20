import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { AuthService } from '../../core/services/auth';
import { LabAssetService } from '../../core/services/lab-asset';
import { LabAsset, CreateLabAssetRequest, AssetStatus } from '../../core/models/lab-asset';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, TableModule, ButtonModule, InputTextModule, DialogModule, TagModule],
  templateUrl: './dashboard.html'
})
export class DashboardComponent implements OnInit {
  authService = inject(AuthService);
  private assetService = inject(LabAssetService);

  assets = signal<LabAsset[]>([]);
  isLoading = signal<boolean>(true);

  // Starea pentru Modal (Pop-up de Adăugare/Editare)
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
        console.error('Error loading assets', err);
        this.isLoading.set(false);
      }
    });
  }

  get isTeacher(): boolean {
    return this.authService.currentUser()?.role === 'Teacher';
  }

  // Mapăm Enum-ul la culorile din PrimeNG
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

  // --- Operații CRUD (Doar Profesori) ---

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
      // Apel pentru Update (Editare)
      this.assetService.update(this.currentAssetId, this.assetForm).subscribe({
        next: () => {
          this.showSuccessMessage();
          this.loadAssets();
          this.hideDialog();
        },
        error: (err: any) => this.handleError(err) // Am adăugat explicit ': any'
      });
    } else {
      // Apel pentru Create (Adăugare nouă)
      this.assetService.create(this.assetForm).subscribe({
        next: () => {
          this.showSuccessMessage();
          this.loadAssets();
          this.hideDialog();
        },
        error: (err: any) => this.handleError(err) // Am adăugat explicit ': any'
      });
    }
  }

  private validateForm(): boolean {
    return !!this.assetForm.name && this.assetForm.name.trim().length > 0;
  }

  private showSuccessMessage() {
    console.log('Operation successful');
  }

  private handleError(err: any) {
    console.error('API Error:', err);
    alert('An error occurred. Please try again.');
  }

  deleteAsset(asset: LabAsset) {
    if (confirm(`Are you sure you want to delete ${asset.name}?`)) {
      this.assetService.deactivate(asset.id).subscribe({
        next: () => this.loadAssets(),
        error: (err) => alert('Error deleting asset')
      });
    }
  }
}