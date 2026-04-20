import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BackendConfigService } from './backend-config';
import { Router } from '@angular/router';
import { tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private backendConfig = inject(BackendConfigService);
  private router = inject(Router);

  isLoggedIn = signal<boolean>(!!localStorage.getItem('token'));

  // Extragem datele utilizatorului direct din token-ul JWT
  currentUser = computed(() => {
    if (!this.isLoggedIn()) return null;
    
    const token = localStorage.getItem('token');
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
      console.error('Error decoding token', e);
      return null;
    }
  });

  login(credentials: { email: string; password: string }) {
    return this.http.post('/api/auth/login', credentials).pipe(
      tap((response: any) => {
        if (response && response.token) {
          localStorage.setItem('token', response.token);
          this.isLoggedIn.set(true);
        }
      })
    );
  }

  register(userData: { fullName: string; email: string; matriculationNumber?: string | null; password: string }) {
    return this.http.post('/api/auth/register', userData);
  }

  logout() {
    localStorage.removeItem('token');
    this.isLoggedIn.set(false);
    this.router.navigate(['/']);
  }
}