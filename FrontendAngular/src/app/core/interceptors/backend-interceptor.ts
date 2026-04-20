import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { BackendConfigService } from '../services/backend-config';

export const backendInterceptor: HttpInterceptorFn = (req, next) => {
  const backendConfig = inject(BackendConfigService);
  
  // 1. Ignorăm fișierele locale (imagini SVG, assets)
  if (req.url.startsWith('./') || req.url.startsWith('assets/')) {
    return next(req);
  }

  // 2. Construim URL-ul complet către backend (Clean sau Vertical)
  let apiReq = req.clone({
    url: `${backendConfig.currentUrl()}${req.url.startsWith('/') ? '' : '/'}${req.url}`
  });

  // 3. Extragem token-ul din localStorage și îl punem în Header (AICI ERA PROBLEMA!)
  const token = localStorage.getItem('token');
  if (token) {
    apiReq = apiReq.clone({
      headers: apiReq.headers.set('Authorization', `Bearer ${token}`)
    });
  }

  // 4. Trimitem request-ul final
  return next(apiReq);
};