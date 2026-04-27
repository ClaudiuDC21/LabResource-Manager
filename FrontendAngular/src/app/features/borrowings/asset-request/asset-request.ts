import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ToastModule } from 'primeng/toast';
import { DatePickerModule } from 'primeng/datepicker';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';

import { LabAssetService } from '../../../core/services/lab-asset.service';
import { BorrowingService } from '../../../core/services/borrowing.service';
import { AuthService } from '../../../core/services/auth.service';
import { LabAsset } from '../../../core/models/lab-asset';
import { AssetHistoryResponse } from '../../../core/models/borrowing';
import { BorrowingStatus } from '../../../core/models/enums';

@Component({
  selector: 'app-asset-request',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    DatePickerModule,
    ButtonModule,
    CardModule,
    ToastModule,
    ProgressSpinnerModule,
    TagModule
  ],
  providers: [MessageService],
  templateUrl: './asset-request.html'
})
export class AssetRequestComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly assetService = inject(LabAssetService);
  private readonly borrowingService = inject(BorrowingService);
  readonly authService = inject(AuthService);
  private readonly messageService = inject(MessageService);

  asset = signal<LabAsset | null>(null);
  assetSchedule = signal<AssetHistoryResponse[]>([]);
  isLoading = signal<boolean>(true);
  isSubmitting = signal<boolean>(false);

  pickUpDate = signal<Date>(new Date());
  pickUpTime = signal<Date>(new Date());
  returnDate = signal<Date>(new Date());
  returnTime = signal<Date>(new Date());
  minDate: Date = new Date();

  bookedSlots = computed(() => {
    const targetDate = this.pickUpDate();
    if (!targetDate) return [];

    const startOfDay = new Date(targetDate).setHours(0, 0, 0, 0);
    const endOfDay = new Date(targetDate).setHours(23, 59, 59, 999);
    const slots: { start: Date, end: Date }[] = [];

    for (const booking of this.assetSchedule()) {
      const bStart = new Date(booking.requestedStartDate);
      const bEnd = new Date(booking.requestedEndDate);

      if (bStart.getTime() <= endOfDay && bEnd.getTime() >= startOfDay) {
        slots.push({
          start: bStart.getTime() < startOfDay ? new Date(startOfDay) : bStart,
          end: bEnd.getTime() > endOfDay ? new Date(endOfDay) : bEnd
        });
      }
    }
    return slots.sort((a, b) => a.start.getTime() - b.start.getTime());
  });

  ngOnInit() {
    const assetId = this.route.snapshot.paramMap.get('id');
    if (assetId) {
      this.loadAsset(assetId);
      this.loadSchedule(assetId);
    } else {
      this.router.navigate(['/dashboard']);
    }

    const defaultReturn = new Date(this.pickUpDate().getTime() + (2 * 60 * 60 * 1000));
    this.returnDate.set(new Date(defaultReturn));
    this.returnTime.set(new Date(defaultReturn));
  }

  loadAsset(id: string) {
    this.assetService.getById(id).subscribe({
      next: (data) => {
        this.asset.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Equipment details could not be loaded.' });
        setTimeout(() => this.router.navigate(['/dashboard']), 2000);
      }
    });
  }

  loadSchedule(id: string) {
    this.borrowingService.getAssetHistory(id).subscribe({
      next: (history) => {
        const activeBookings = history.filter(h => 
          h.status !== BorrowingStatus.Returned && 
          h.status !== BorrowingStatus.Rejected
        );
        this.assetSchedule.set(activeBookings);
      }
    });
  }

  // --- FUNCȚIA NOUĂ: Sincronizează Pick-Up cu Return ---
  onPickUpDateChange(newDate: Date) {
    if (!newDate) return;
    this.pickUpDate.set(newDate);
    // Setăm data de return pe exact aceeași zi, instanțiind un obiect nou de Date
    this.returnDate.set(new Date(newDate.getTime()));
  }

  getDayStatus(dateObj: any): 'full' | 'partial' | 'free' {
    const checkDate = new Date(dateObj.year, dateObj.month, dateObj.day);
    const startOfDay = new Date(checkDate).setHours(0, 0, 0, 0);
    const endOfDay = new Date(checkDate).setHours(23, 59, 59, 999);

    let totalBookedMS = 0;

    for (const booking of this.assetSchedule()) {
      const bStart = new Date(booking.requestedStartDate).getTime();
      const bEnd = new Date(booking.requestedEndDate).getTime();

      if (bStart <= endOfDay && bEnd >= startOfDay) {
        const overlapStart = Math.max(startOfDay, bStart);
        const overlapEnd = Math.min(endOfDay, bEnd);
        totalBookedMS += (overlapEnd - overlapStart);
      }
    }

    if (totalBookedMS === 0) return 'free';
    
    const msInDay = 24 * 60 * 60 * 1000;
    if (totalBookedMS >= msInDay * 0.9) return 'full'; 

    return 'partial';
  }

  private combineDateTime(dateSource: Date, timeSource: Date): Date {
    const combined = new Date(dateSource);
    combined.setHours(timeSource.getHours());
    combined.setMinutes(timeSource.getMinutes());
    combined.setSeconds(0);
    combined.setMilliseconds(0);
    return combined;
  }

  private toLocalISOString(date: Date): string {
    const pad = (n: number) => n.toString().padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}:00`;
  }

  submitRequest() {
    if (!this.asset()) return;

    const finalStart = this.combineDateTime(this.pickUpDate(), this.pickUpTime());
    const finalEnd = this.combineDateTime(this.returnDate(), this.returnTime());

    if (finalStart < new Date(new Date().getTime() - 1000 * 60 * 5)) {
       this.messageService.add({ severity: 'warn', summary: 'Invalid Date', detail: 'Pick-up time cannot be in the past.' });
      return;
    }

    if (finalStart >= finalEnd) {
      this.messageService.add({ severity: 'warn', summary: 'Invalid Range', detail: 'Return time must be after the pick-up time.' });
      return;
    }

    this.isSubmitting.set(true);
    const user = this.authService.currentUser();

    const requestPayload = {
      userId: user!.id,
      labAssetId: this.asset()!.id,
      requestedStartDate: this.toLocalISOString(finalStart),
      requestedEndDate: this.toLocalISOString(finalEnd)
    };

    this.borrowingService.requestAsset(requestPayload).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Your reservation request has been submitted!' });
        setTimeout(() => this.router.navigate(['/dashboard']), 1500);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        const errorDetail = err.error?.Error || 'The equipment is already reserved for the selected interval.';
        this.messageService.add({ severity: 'error', summary: 'Request Failed', detail: errorDetail });
      }
    });
  }
}