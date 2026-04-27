import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { BackendConfigService } from '../services/backend-config.service';

export const backendInterceptor: HttpInterceptorFn = (req, next) => {
  const backendConfig = inject(BackendConfigService);
  
  if (req.url.startsWith('./') || req.url.startsWith('assets/')) {
    return next(req);
  }

  let apiReq = req.clone({
    url: `${backendConfig.currentUrl()}${req.url.startsWith('/') ? '' : '/'}${req.url}`
  });

  const token = localStorage.getItem('token') || sessionStorage.getItem('token');
  
  if (token) {
    apiReq = apiReq.clone({
      headers: apiReq.headers.set('Authorization', `Bearer ${token}`)
    });
  }

  return next(apiReq);
};