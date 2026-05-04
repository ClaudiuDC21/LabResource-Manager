import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ProgressBarModule } from 'primeng/progressbar';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

import { LabAssetService } from '../../../core/services/lab-asset.service';
import { BorrowingService } from '../../../core/services/borrowing.service';
import { AssetStatus, BorrowingStatus } from '../../../core/models/enums';
import { AssetHistoryResponse } from '../../../core/models/borrowing';
import { LabAsset } from '../../../core/models/lab-asset';
import { UIHelpers } from '../../../core/models/ui-helpers';

export interface MappedAssetActive extends AssetHistoryResponse {
  progressValue: number;
  timeLeftLabel: string;
  isExceeded: boolean;
}

export interface MappedAssetHistory extends AssetHistoryResponse {
  timelinessLabel: string;
  timelinessSeverity: 'success' | 'danger' | 'info';
}

@Component({
  selector: 'app-asset-details',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    CardModule,
    ButtonModule,
    TableModule,
    TagModule,
    ProgressBarModule,
    ProgressSpinnerModule,
    ToastModule
  ],
  providers: [MessageService],
  templateUrl: './asset-details.html'
})
export class AssetDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly assetService = inject(LabAssetService);
  private readonly borrowingService = inject(BorrowingService);
  private readonly messageService = inject(MessageService);

  public readonly BorrowingStatus = BorrowingStatus;
  public readonly AssetStatus = AssetStatus;
  public readonly UIHelpers = UIHelpers;

  asset = signal<LabAsset | null>(null);
  upcomingAndActive = signal<MappedAssetActive[]>([]);
  pastHistory = signal<MappedAssetHistory[]>([]);
  
  isLoading = signal<boolean>(true);

  ngOnInit() {
    const assetId = this.route.snapshot.paramMap.get('id');
    if (assetId) {
      this.loadAssetDetails(assetId);
    }
  }

  loadAssetDetails(id: string) {
    this.isLoading.set(true);

    this.assetService.getById(id).subscribe({
      next: (data) => this.asset.set(data),
      error: () => this.showError('Failed to load asset details.')
    });

    this.borrowingService.getAssetHistory(id).subscribe({
      next: (history) => {
        const future = history.filter(h => 
          h.status === BorrowingStatus.Pending || 
          h.status === BorrowingStatus.Approved || 
          h.status === BorrowingStatus.Active
        );
        
        const past = history.filter(h => 
          h.status === BorrowingStatus.Returned || 
          h.status === BorrowingStatus.Rejected
        );

        this.upcomingAndActive.set(future.map(h => ({
          ...h,
          ...UIHelpers.calculateProgress(h.requestedStartDate, h.requestedEndDate, h.status)
        })));
        
        this.pastHistory.set(past.map(h => {
          let label = '-';
          let severity: 'success' | 'danger' | 'info' = 'info';

          if (h.status === BorrowingStatus.Returned && h.actualReturnedAt) {
             const expected = new Date(h.requestedEndDate).getTime();
             const actual = new Date(h.actualReturnedAt).getTime();
             if (actual > expected) {
                label = 'Exceeded';
                severity = 'danger';
             } else {
                label = 'On Time';
                severity = 'success';
             }
          } else if (h.status === BorrowingStatus.Rejected) {
             label = 'N/A';
          }

          return {
              ...h,
              timelinessLabel: label,
              timelinessSeverity: severity
          };
        }));
        this.isLoading.set(false);
      },
      error: () => {
        this.showError('Failed to load asset history.');
        this.isLoading.set(false);
      }
    });
  }

  private showError(msg: string) {
    this.messageService.add({ severity: 'error', summary: 'Error', detail: msg });
  }
}