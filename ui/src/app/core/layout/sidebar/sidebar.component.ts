import { ChangeDetectionStrategy, Component, input, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ChevronLeft, LayoutGrid, LucideAngularModule } from 'lucide-angular';
import { NavItem } from '../nav';

/** Ports the React App.tsx <aside> sidebar (collapse + mobile overlay). */
@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive, LucideAngularModule],
  templateUrl: './sidebar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent {
  readonly items = input.required<NavItem[]>();

  protected readonly LayoutGrid = LayoutGrid;
  protected readonly ChevronLeft = ChevronLeft;

  readonly collapsed = signal(false);
  readonly mobileOpen = signal(false);

  openMobile(): void {
    this.mobileOpen.set(true);
  }
}
