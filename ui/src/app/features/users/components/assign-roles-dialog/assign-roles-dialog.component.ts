import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import {
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
  LucideAngularModule,
  Search,
} from 'lucide-angular';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { injectAssignRoles, injectUserRolesQuery } from '../../data/users.queries';
import { KeycloakUser, RealmRole } from '../../models/user.model';

export interface AssignRolesData {
  user: KeycloakUser;
  realmRoles: RealmRole[];
}

/** Ports AssignRolesModal — dual-list transfer with per-list search. */
@Component({
  selector: 'app-assign-roles-dialog',
  imports: [ButtonModule, InputTextModule, ProgressSpinnerModule, LucideAngularModule],
  templateUrl: './assign-roles-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssignRolesDialogComponent {
  private readonly ref = inject(DynamicDialogRef);
  private readonly config = inject(DynamicDialogConfig);
  readonly data = this.config.data as AssignRolesData;

  protected readonly Search = Search;
  protected readonly ChevronLeft = ChevronLeft;
  protected readonly ChevronRight = ChevronRight;
  protected readonly ChevronsLeft = ChevronsLeft;
  protected readonly ChevronsRight = ChevronsRight;

  private readonly userId = signal(this.data.user.id);
  readonly rolesQuery = injectUserRolesQuery(this.userId);
  readonly mut = injectAssignRoles();

  private readonly initial = signal<string[] | null>(null);
  private readonly assigned = signal<Set<string>>(new Set());
  protected readonly availableSelected = signal<Set<string>>(new Set());
  protected readonly assignedSelected = signal<Set<string>>(new Set());
  protected readonly availableSearch = signal('');
  protected readonly assignedSearch = signal('');

  constructor() {
    // Seed the assigned set once the user's current roles load.
    effect(() => {
      const roles = this.rolesQuery.data();
      if (roles && this.initial() === null) {
        this.initial.set(roles);
        this.assigned.set(new Set(roles));
      }
    });
    // If the user's roles fail to load, close the dialog (parent shows nothing extra).
    effect(() => {
      if (this.rolesQuery.isError()) this.ref.close(false);
    });
  }

  readonly availableRoles = computed(() =>
    this.filter(
      this.data.realmRoles.filter((r) => !this.assigned().has(r.name)),
      this.availableSearch(),
    ),
  );
  readonly assignedRoles = computed(() =>
    this.filter(
      this.data.realmRoles.filter((r) => this.assigned().has(r.name)),
      this.assignedSearch(),
    ),
  );
  readonly assignedCount = computed(() => this.assigned().size);
  readonly dirty = computed(() => {
    const init = this.initial();
    if (init === null) return false;
    const a = this.assigned();
    return a.size !== init.length || init.some((r) => !a.has(r));
  });

  private filter(list: RealmRole[], q: string): RealmRole[] {
    return q ? list.filter((r) => r.name.toLowerCase().includes(q.toLowerCase())) : list;
  }

  protected asArray(s: Set<string>): string[] {
    return [...s];
  }
  protected names(roles: RealmRole[]): string[] {
    return roles.map((r) => r.name);
  }

  protected toggle(sel: ReturnType<typeof signal<Set<string>>>, name: string): void {
    const next = new Set(sel());
    next.has(name) ? next.delete(name) : next.add(name);
    sel.set(next);
  }

  protected moveToAssigned(names: string[]): void {
    if (!names.length) return;
    const next = new Set(this.assigned());
    names.forEach((n) => next.add(n));
    this.assigned.set(next);
    this.availableSelected.set(new Set());
  }

  protected moveToAvailable(names: string[]): void {
    if (!names.length) return;
    const next = new Set(this.assigned());
    names.forEach((n) => next.delete(n));
    this.assigned.set(next);
    this.assignedSelected.set(new Set());
  }

  async save(): Promise<void> {
    try {
      await this.mut.mutateAsync({ id: this.data.user.id, dto: { roles: [...this.assigned()] } });
      this.ref.close(true);
    } catch {
      // toast already shown by onError
    }
  }

  cancel(): void {
    this.ref.close(false);
  }
}
