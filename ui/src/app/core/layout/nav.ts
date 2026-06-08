import { Boxes, LucideIconData, Package, ShoppingBag, Users } from 'lucide-angular';

export interface NavItem {
  id: 'products' | 'orders' | 'inventory' | 'users';
  label: string;
  icon: LucideIconData;
  description: string;
  adminOnly?: boolean;
}

/** Ports `allNavItems` from the React App.tsx. */
export const ALL_NAV: NavItem[] = [
  { id: 'products', label: 'Products', icon: Package, description: 'Manage product catalog' },
  { id: 'orders', label: 'Orders', icon: ShoppingBag, description: 'View and manage orders' },
  {
    id: 'inventory',
    label: 'Inventory',
    icon: Boxes,
    description: 'Track on-hand and reserved stock',
  },
  {
    id: 'users',
    label: 'Users',
    icon: Users,
    description: 'Manage Keycloak users (admin only)',
    adminOnly: true,
  },
];
