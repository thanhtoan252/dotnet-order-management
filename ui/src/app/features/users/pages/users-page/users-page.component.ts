import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { LucideAngularModule, Plus, RefreshCw, Search } from 'lucide-angular';
import { ButtonModule } from 'primeng/button';
import { DialogService } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { PermissionsService } from '../../../../core/auth/permissions.service';
import { ConfirmDialogComponent } from '../../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { AssignRolesDialogComponent } from '../../components/assign-roles-dialog/assign-roles-dialog.component';
import { CreateUserDialogComponent } from '../../components/create-user-dialog/create-user-dialog.component';
import { EditUserDialogComponent } from '../../components/edit-user-dialog/edit-user-dialog.component';
import { ResetPasswordDialogComponent } from '../../components/reset-password-dialog/reset-password-dialog.component';
import { UsersTableComponent } from '../../components/users-table/users-table.component';
import {
  injectDeleteUser,
  injectRealmRolesQuery,
  injectUsersQuery,
} from '../../data/users.queries';
import { KeycloakUser } from '../../models/user.model';

/** Orchestrates the users list, search, and all user dialogs. Ports UsersManager. */
@Component({
  selector: 'app-users-page',
  imports: [ButtonModule, InputTextModule, LucideAngularModule, UsersTableComponent],
  templateUrl: './users-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsersPageComponent {
  protected readonly perms = inject(PermissionsService);
  private readonly dialog = inject(DialogService);

  protected readonly Search = Search;
  protected readonly RefreshCw = RefreshCw;
  protected readonly Plus = Plus;

  protected readonly searchInput = signal('');
  private readonly appliedSearch = signal('');

  readonly users = injectUsersQuery(this.appliedSearch);
  private readonly realmRoles = injectRealmRolesQuery();
  private readonly deleteMut = injectDeleteUser();

  count(): number {
    return this.users.data()?.length ?? 0;
  }

  applySearch(event?: Event): void {
    event?.preventDefault();
    // If the applied term is unchanged, force a refetch (Refresh button behaviour).
    if (this.appliedSearch() === this.searchInput()) {
      this.users.refetch();
    } else {
      this.appliedSearch.set(this.searchInput());
    }
  }

  openCreate(): void {
    this.dialog.open(CreateUserDialogComponent, {
      header: 'New User',
      width: '520px',
      modal: true,
      dismissableMask: true,
      data: { realmRoles: this.realmRoles.data() ?? [] },
    });
  }

  openEdit(user: KeycloakUser): void {
    this.dialog.open(EditUserDialogComponent, {
      header: 'Edit User',
      width: '520px',
      modal: true,
      dismissableMask: true,
      data: user,
    });
  }

  openReset(user: KeycloakUser): void {
    this.dialog.open(ResetPasswordDialogComponent, {
      header: 'Reset Password',
      width: '440px',
      modal: true,
      dismissableMask: true,
      data: user,
    });
  }

  openRoles(user: KeycloakUser): void {
    this.dialog.open(AssignRolesDialogComponent, {
      header: 'Manage Roles',
      width: '760px',
      modal: true,
      dismissableMask: true,
      data: { user, realmRoles: this.realmRoles.data() ?? [] },
    });
  }

  confirmDelete(user: KeycloakUser): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      header: 'Delete User',
      width: '440px',
      modal: true,
      dismissableMask: true,
      data: {
        title: 'Delete User',
        message: `Delete user "${user.username}"? This permanently removes them from Keycloak and cannot be undone.`,
        confirmText: 'Delete User',
        danger: true,
      },
    });
    ref?.onClose.subscribe((confirmed) => {
      if (confirmed) this.deleteMut.mutate(user.id);
    });
  }
}
