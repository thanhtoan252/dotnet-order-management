import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { PermissionsService } from './permissions.service';

/** Blocks unauthenticated access; redirects to /login. */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.isAuthenticated() ? true : inject(Router).createUrlTree(['/login']);
};

/** Admin-only routes (e.g. /users). Non-admins are sent back to /products. */
export const adminGuard: CanActivateFn = () => {
  const perms = inject(PermissionsService);
  return perms.canManageUsers() ? true : inject(Router).createUrlTree(['/products']);
};
