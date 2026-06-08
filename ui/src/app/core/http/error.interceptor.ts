import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { ApiError, ProblemDetails } from './api-error';

/**
 * Normalizes failures into `ApiError` and handles 401 globally.
 * Ports `lib/api/handleResponse.ts`: on 401 the React app cleared storage and
 * did a hard `window.location.reload()`; here we logout and SPA-navigate to /login.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  return next(req).pipe(
    catchError((e: HttpErrorResponse) => {
      if (e.status === 401) {
        auth.logout();
      }
      const problems = (e.error ?? undefined) as ProblemDetails | undefined;
      return throwError(() => new ApiError(e.status, problems));
    }),
  );
};
