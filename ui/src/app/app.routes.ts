import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './core/auth/guards';
import { ShellComponent } from './core/layout/shell/shell.component';

export const routes: Routes = [
  {
    path: 'login',
    loadChildren: () => import('./features/auth/auth.routes'),
  },
  {
    path: '',
    canActivate: [authGuard],
    component: ShellComponent,
    children: [
      { path: 'products', loadChildren: () => import('./features/products/products.routes') },
      { path: 'inventory', loadChildren: () => import('./features/inventory/inventory.routes') },
      { path: 'orders', loadChildren: () => import('./features/orders/orders.routes') },
      {
        path: 'users',
        loadChildren: () => import('./features/users/users.routes'),
        canActivate: [adminGuard],
      },
      { path: '', redirectTo: 'products', pathMatch: 'full' },
    ],
  },
  { path: '**', redirectTo: '' },
];
