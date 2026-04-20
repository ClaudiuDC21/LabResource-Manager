import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  BorrowAssetRequest, 
  ReturnAssetRequest, 
  BorrowingResponse, 
  ReturnAssetResponse, 
  ActiveBorrowingResponse, 
  AssetHistoryResponse 
} from '../models/borrowing';

@Injectable({ providedIn: 'root' })
export class BorrowingService {
  private http = inject(HttpClient);
  
  // The interceptor will handle prepending https://localhost:6001 or 5001
  private apiUrl = '/api/Borrowings';

  borrow(request: BorrowAssetRequest): Observable<BorrowingResponse> {
    return this.http.post<BorrowingResponse>(`${this.apiUrl}/borrow`, request);
  }

  returnAsset(request: ReturnAssetRequest): Observable<ReturnAssetResponse> {
    return this.http.post<ReturnAssetResponse>(`${this.apiUrl}/return`, request);
  }

  getActiveForUser(userId: string): Observable<ActiveBorrowingResponse[]> {
    return this.http.get<ActiveBorrowingResponse[]>(`${this.apiUrl}/user/${userId}/active`);
  }

  getAssetHistory(assetId: string): Observable<AssetHistoryResponse[]> {
    return this.http.get<AssetHistoryResponse[]>(`${this.apiUrl}/asset/${assetId}/history`);
  }
}