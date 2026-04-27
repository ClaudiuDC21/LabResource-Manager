export interface AuthResponse {
  token: string;
  email: string;
  fullName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterUserRequest {
  fullName: string;
  email: string;
  matriculationNumber?: string | null;
  password: string;
}