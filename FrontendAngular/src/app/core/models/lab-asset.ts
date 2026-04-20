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
  
  // Câmpurile noi pentru informațiile de împrumut
  currentBorrowerName?: string;
  currentBorrowDate?: string; // Va veni ca un ISO string (ex: "2026-04-20T10:00:00Z")
}

export interface CreateLabAssetRequest {
  name: string;
  serialNumber?: string | null;
}