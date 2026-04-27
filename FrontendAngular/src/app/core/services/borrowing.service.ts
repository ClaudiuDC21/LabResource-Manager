import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { 
  BorrowAssetRequest, 
  ReturnAssetRequest, 
  BorrowingResponse, 
  ReturnAssetResponse, 
  ActiveBorrowingResponse, 
  AssetHistoryResponse,
  ReviewBorrowingRequest,
  UserBorrowingHistoryResponse
} from '../models/borrowing';

@Injectable({ providedIn: 'root' })
export class BorrowingService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/Borrowings';

  private pendingCountUpdatedSource = new Subject<void>();
  public pendingCountUpdated$ = this.pendingCountUpdatedSource.asObservable();

  notifyPendingCountChanged() {
    this.pendingCountUpdatedSource.next();
  }

  requestAsset(request: BorrowAssetRequest): Observable<BorrowingResponse> {
    return this.http.post<BorrowingResponse>(`${this.apiUrl}/request`, request);
  }

  reviewRequest(borrowingId: string, request: ReviewBorrowingRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${borrowingId}/review`, request);
  }

  pickUpAsset(borrowingId: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${borrowingId}/pickup`, {});
  }

  returnAsset(borrowingId: string, request: ReturnAssetRequest): Observable<ReturnAssetResponse> {
    return this.http.post<ReturnAssetResponse>(`${this.apiUrl}/${borrowingId}/return`, request);
  }

  getActiveForUser(userId: string): Observable<ActiveBorrowingResponse[]> {
    return this.http.get<ActiveBorrowingResponse[]>(`${this.apiUrl}/user/${userId}/active`);
  }

  getAssetHistory(assetId: string): Observable<AssetHistoryResponse[]> {
    return this.http.get<AssetHistoryResponse[]>(`${this.apiUrl}/asset/${assetId}/history`);
  }

  getUserHistory(userId: string): Observable<UserBorrowingHistoryResponse[]> {
    return this.http.get<UserBorrowingHistoryResponse[]>(`${this.apiUrl}/user/${userId}/history`);
  }

  getPendingForTeacher(teacherId: string): Observable<ActiveBorrowingResponse[]> {
    return this.http.get<ActiveBorrowingResponse[]>(`${this.apiUrl}/teacher/${teacherId}/pending`);
  }
}