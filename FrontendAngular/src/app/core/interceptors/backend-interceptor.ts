import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { BackendConfigService } from '../services/backend-config';

export const backendInterceptor: HttpInterceptorFn = (req, next) => {
  const backendConfig = inject(BackendConfigService);
  
  if (req.url.startsWith('./') || req.url.startsWith('assets/')) {
    return next(req);
  }

  const apiReq = req.clone({
    url: `${backendConfig.currentUrl()}${req.url.startsWith('/') ? '' : '/'}${req.url}`
  });

  return next(apiReq);
};