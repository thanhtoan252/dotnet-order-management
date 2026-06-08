import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import {
  CheckCircle2,
  KeyRound,
  LucideAngularModule,
  Pencil,
  ShieldCheck,
  Trash2,
  Users as UsersIcon,
  XCircle,
} from 'lucide-angular';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { KeycloakUser } from '../../models/user.model';

/** Users list on a sortable PrimeNG p-table with status + role chips. */
@Component({
  selector: 'app-users-table',
  imports: [TableModule, TooltipModule, LucideAngularModule],
  templateUrl: './users-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsersTableComponent {
  readonly users = input.required<KeycloakUser[]>();
  readonly loading = input(false);
  readonly canManage = input(false);
  readonly edit = output<KeycloakUser>();
  readonly remove = output<KeycloakUser>();
  readonly resetPassword = output<KeycloakUser>();
  readonly manageRoles = output<KeycloakUser>();

  protected readonly UsersIcon = UsersIcon;
  protected readonly CheckCircle2 = CheckCircle2;
  protected readonly XCircle = XCircle;
  protected readonly Pencil = Pencil;
  protected readonly ShieldCheck = ShieldCheck;
  protected readonly KeyRound = KeyRound;
  protected readonly Trash2 = Trash2;

  protected fullName(u: KeycloakUser): string {
    return [u.firstName, u.lastName].filter(Boolean).join(' ');
  }
}
