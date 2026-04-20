export enum AssetStatus {
  Available = 1,
  Borrowed = 2,
  Defective = 3
}

export interface LabAsset {
  id: string;
  name: string;
  serialNumber?: string;
  status: AssetStatus;
  isActive: boolean;
}

export interface CreateLabAssetRequest {
  name: string;
  serialNumber?: string | null;
}