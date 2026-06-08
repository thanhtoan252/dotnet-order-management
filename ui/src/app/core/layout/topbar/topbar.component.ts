import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { LogOut, LucideAngularModule, Menu, User } from 'lucide-angular';
import { NavItem } from '../nav';

/** Ports the React App.tsx <header> top bar. */
@Component({
  selector: 'app-topbar',
  imports: [LucideAngularModule],
  templateUrl: './topbar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TopbarComponent {
  readonly current = input.required<NavItem>();
  readonly username = input.required<string>();
  readonly logout = output<void>();
  readonly menu = output<void>();

  protected readonly Menu = Menu;
  protected readonly User = User;
  protected readonly LogOut = LogOut;
}
