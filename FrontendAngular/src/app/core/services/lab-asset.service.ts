import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BackendConfigService } from './backend-config.service';
import { LabAsset, CreateLabAssetRequest } from '../models/lab-asset';

@Injectable({ providedIn: 'root' })
export class LabAssetService {
  private readonly http = inject(HttpClient);
  private readonly backendConfig = inject(BackendConfigService);
  
  private readonly apiUrl = '/api/LabAssets';

  getAllActive(): Observable<LabAsset[]> {
    return this.http.get<LabAsset[]>(this.apiUrl);
  }

  getById(id: string): Observable<LabAsset> {
    return this.http.get<LabAsset>(`${this.apiUrl}/${id}`);
  }

  create(asset: CreateLabAssetRequest): Observable<LabAsset> {
    return this.http.post<LabAsset>(this.apiUrl, asset);
  }

  update(id: string, asset: CreateLabAssetRequest): Observable<void> {
    const payload = {
      name: asset.name,
      serialNumber: asset.serialNumber,
      location: asset.location,
      assignedTeacherId: asset.assignedTeacherId
    };

    if (this.backendConfig.isCleanArchitecture()) {
      return this.http.put<void>(`${this.apiUrl}/${id}`, payload);
    } else {
      return this.http.put<void>(`${this.apiUrl}/${id}`, {
        ...payload,
        id: id
      });
    }
  }

  deactivate(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}