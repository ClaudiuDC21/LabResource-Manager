import { BorrowingStatus, AssetStatus } from './enums';

export interface BorrowAssetRequest {
  userId: string;
  labAssetId: string;
  requestedStartDate: string | Date;
  requestedEndDate: string | Date;
}

export interface ReviewBorrowingRequest {
  isApproved: boolean;
  teacherNotes?: string | null;
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
  assetName: string;
  userName: string;
  requestedStartDate: string;
  requestedEndDate: string;
  status: BorrowingStatus;
}

export interface ActiveBorrowingResponse {
  borrowingRecordId: string;
  labAssetId: string;
  assetName: string;
  userName?: string;
  serialNumber?: string | null;
  requestedStartDate: string;
  requestedEndDate: string;
  status: BorrowingStatus;
}

export interface AssetHistoryResponse {
  borrowingRecordId: string;
  userName: string;
  matriculationNumber?: string | null;
  requestedStartDate: string;
  requestedEndDate: string;
  actualReturnedAt?: string | null;
  status: BorrowingStatus;
  remarks?: string | null;
}

export interface UserBorrowingHistoryResponse {
  assetName: string;
  serialNumber?: string | null;
  requestedStartDate: string;
  requestedEndDate: string;
  actualReturnedAt?: string | null;
  status: BorrowingStatus;
  isDefective: boolean;
  remarks?: string | null;
}

export interface ReturnAssetResponse {
  borrowingRecordId: string;
  assetName: string;
  returnedAt: string;
  newStatus: AssetStatus;
}