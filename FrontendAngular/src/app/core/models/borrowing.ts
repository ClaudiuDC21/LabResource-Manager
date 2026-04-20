import { AssetStatus } from './lab-asset';

export interface BorrowAssetRequest {
  userId: string;
  labAssetId: string;
}

export interface ReturnAssetRequest {
  labAssetId: string;
  remarks?: string | null;
  isDefective: boolean;
}

export interface BorrowingResponse {
  id: string;
  userId: string;
  labAssetId: string;
  borrowedAt: string;
  assetName: string;
  userName: string;
}

export interface ActiveBorrowingResponse {
  borrowingRecordId: string;
  labAssetId: string;
  assetName: string;
  serialNumber?: string | null;
  borrowedAt: string;
}

export interface AssetHistoryResponse {
  borrowingRecordId: string;
  userName: string;
  matriculationNumber?: string | null;
  borrowedAt: string;
  returnedAt?: string | null;
  remarks?: string | null;
}

export interface ReturnAssetResponse {
  borrowingRecordId: string;
  assetName: string;
  returnedAt: string;
  newStatus: AssetStatus;
}