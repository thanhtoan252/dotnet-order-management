import { computed, inject, Injectable } from '@angular/core';
import { AuthService } from './auth.service';

/**
 * Role-based capability flags. Ports the React `usePermissions` hook.
 * Currently every capability maps to the `admin` realm role.
 */
@Injectable({ providedIn: 'root' })
export class PermissionsService {
  private readonly roles = inject(AuthService).roles;
  private readonly isAdmin = computed(() => this.roles().includes('admin'));

  readonly canManageProducts = this.isAdmin;
  readonly canManageOrders = this.isAdmin;
  readonly canManageInventory = this.isAdmin;
  readonly canManageUsers = this.isAdmin;
}
