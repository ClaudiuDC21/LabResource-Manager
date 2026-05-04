import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  UserResponse, 
  UpdateUserRequest, 
  UpdatePasswordRequest
} from '../models/user';
import { RegisterUserRequest } from '../models/auth';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/Users';

  register(request: RegisterUserRequest): Observable<UserResponse> {
    return this.http.post<UserResponse>(`${this.apiUrl}/register`, request);
  }

  getAllActive(): Observable<UserResponse[]> {
    return this.http.get<UserResponse[]>(this.apiUrl);
  }

  getById(id: string): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.apiUrl}/${id}`);
  }

  update(id: string, request: UpdateUserRequest): Observable<void> {
    const payload = { ...request, id: id };
    return this.http.put<void>(`${this.apiUrl}/${id}`, payload);
  }

  updatePassword(id: string, request: UpdatePasswordRequest): Observable<void> {
    const payload = { ...request, id: id };
    return this.http.put<void>(`${this.apiUrl}/${id}/password`, payload);
  }

  deactivate(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}