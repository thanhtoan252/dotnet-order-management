import { useAuth } from './AuthProvider';

export function usePermissions() {
  const { roles } = useAuth();
  const isAdmin = roles.includes('admin');

  return {
    canManageProducts: isAdmin,
    canManageOrders: isAdmin,
  };
}
