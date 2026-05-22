import { AssetStatus } from './enums';

export interface LabAsset {
  id: string;
  name: string;
  serialNumber?: string | null;
  location?: string | null;
  status: AssetStatus;
  isActive: boolean;
  assignedTeacherId?: string | null;
  assignedTeacherName?: string | null;
  currentBorrowerName?: string | null;
  currentBorrowDate?: string | null;
}

export interface LabAssetResponse extends LabAsset {}

export interface CreateLabAssetRequest {
  name: string;
  serialNumber?: string | null;
  location?: string | null;
  assignedTeacherId?: string | null;
}

export interface UpdateLabAssetRequest {
  name: string;
  serialNumber?: string | null;
  location?: string | null;
  assignedTeacherId?: string | null;
  isDefective: boolean;
}