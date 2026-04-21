import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BackendConfigService } from './backend-config';
import { LabAsset, CreateLabAssetRequest } from '../models/lab-asset';

@Injectable({ providedIn: 'root' })
export class LabAssetService {
  private readonly http = inject(HttpClient);
  private readonly backendConfig = inject(BackendConfigService); // Am injectat serviciul nostru
  
  private readonly apiUrl = '/api/LabAssets';

  getAllActive(): Observable<LabAsset[]> {
    return this.http.get<LabAsset[]>(this.apiUrl);
  }

  create(asset: CreateLabAssetRequest): Observable<LabAsset> {
    return this.http.post<LabAsset>(this.apiUrl, asset);
  }

  update(id: string, asset: CreateLabAssetRequest): Observable<void> {
    // Verificăm la ce arhitectură suntem conectați
    if (this.backendConfig.isCleanArchitecture()) {
      // Clean Architecture vrea doar Name și SerialNumber
      return this.http.put<void>(`${this.apiUrl}/${id}`, {
        name: asset.name,
        serialNumber: asset.serialNumber
      });
    } else {
      // Vertical Slice vrea și ID-ul inclus în Command
      return this.http.put<void>(`${this.apiUrl}/${id}`, {
        id: id,
        name: asset.name,
        serialNumber: asset.serialNumber
      });
    }
  }

  deactivate(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}