import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Toast } from 'primeng/toast';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Toast],
  template: `
    <router-outlet />
    <p-toast position="top-right" />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {}
