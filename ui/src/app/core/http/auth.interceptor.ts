import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TokenStore } from '../auth/token-store';

/**
 * Attaches the bearer token to every outgoing request.
 * Ports the token-attaching part of the React `lib/api/request.ts`.
 * Content-Type is handled automatically by HttpClient (skipped for FormData).
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(TokenStore).token();
  if (!token) {
    return next(req);
  }
  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
