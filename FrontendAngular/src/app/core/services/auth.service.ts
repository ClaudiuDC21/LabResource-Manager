import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BackendConfigService } from './backend-config.service';
import { Router } from '@angular/router';
import { tap } from 'rxjs';

export interface UserState {
  id: string;
  name: string;
  role: string | number;
  email: string;
}
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly backendConfig = inject(BackendConfigService);
  private readonly router = inject(Router);

  isLoggedIn = signal<boolean>(this.checkInitialAuth());

  private currentUserState = signal<UserState | null>(this.extractUserFromToken());

  currentUser = computed(() => this.currentUserState());

  private extractUserFromToken(): UserState | null {
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    if (!token) return null;

    try {
      const payloadBase64 = token.split('.')[1];
      const decodedJson = atob(payloadBase64);
      const payload = JSON.parse(decodedJson);

      return {
        id: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload['sub'] || '',
        name: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'User',
        role: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'Member',
        email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || payload['email'] || 'N/A'
      };
    } catch (e) {
      return null;
    }
  }

  private checkInitialAuth(): boolean {
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      if (payload.exp && (payload.exp * 1000) < Date.now()) {
        localStorage.removeItem('token');
        sessionStorage.removeItem('token');
        return false;
      }
      return true;
    } catch {
      return false;
    }
  }

  updateCurrentUserState(newState: UserState) {
    this.currentUserState.set(newState);
  }

  login(credentials: { email: string; password: string }, rememberMe: boolean = false) {
    return this.http.post('/api/auth/login', credentials).pipe(
      tap((response: any) => {
        if (response?.token) {
          if (rememberMe) {
            localStorage.setItem('token', response.token);
          } else {
            sessionStorage.setItem('token', response.token);
          }
          this.isLoggedIn.set(true);
          this.currentUserState.set(this.extractUserFromToken()); 
        }
      })
    );
  }

  register(userData: { fullName: string; email: string; matriculationNumber?: string | null; password: string }) {
    return this.http.post('/api/users/register', userData);
  }

  logout() {
    localStorage.removeItem('token');
    sessionStorage.removeItem('token');
    this.isLoggedIn.set(false);
    this.currentUserState.set(null); 
    this.router.navigate(['/']);
  }
}