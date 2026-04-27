import { UserRole } from './enums';

export interface UserResponse {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  isActive: boolean;
  matriculationNumber?: string | null;
}

export interface UpdateUserRequest {
  fullName: string;
  matriculationNumber?: string | null;
}

export interface UpdatePasswordRequest {
  currentPassword: string;
  newPassword: string;
}