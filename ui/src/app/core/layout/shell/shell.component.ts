import { ChangeDetectionStrategy, Component, computed, inject, viewChild } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter, map } from 'rxjs';
import { AuthService } from '../../auth/auth.service';
import { PermissionsService } from '../../auth/permissions.service';
import { ALL_NAV } from '../nav';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';

/** App shell (sidebar + topbar + routed content). Ports the React App.tsx layout. */
@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, SidebarComponent, TopbarComponent],
  templateUrl: './shell.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShellComponent {
  protected readonly auth = inject(AuthService);
  private readonly perms = inject(PermissionsService);
  private readonly router = inject(Router);

  protected readonly sidebar = viewChild(SidebarComponent);

  // Filter the nav by permission — ports useMemo(allNavItems.filter(...)).
  readonly navItems = computed(() =>
    ALL_NAV.filter((n) => !n.adminOnly || this.perms.canManageUsers()),
  );

  // Derive the active page from the router URL instead of the URL hash.
  readonly current = toSignal(
    this.router.events.pipe(
      filter((e): e is NavigationEnd => e instanceof NavigationEnd),
      map(() => ALL_NAV.find((n) => this.router.url.startsWith('/' + n.id)) ?? ALL_NAV[0]),
    ),
    { initialValue: ALL_NAV[0] },
  );
}
