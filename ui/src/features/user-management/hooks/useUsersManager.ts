import { useState, useCallback, useEffect } from 'react';
import { toast } from 'sonner';
import { ApiError } from '../../../lib/api';
import type {
  KeycloakUser,
  RealmRole,
  CreateUserRequest,
  UpdateUserRequest,
  ResetPasswordRequest,
  AssignRolesRequest,
  UserSearchParams,
} from '../types';
import {
  fetchUsersApi,
  fetchRealmRolesApi,
  createUserApi,
  updateUserApi,
  deleteUserApi,
  resetPasswordApi,
  assignRolesApi,
} from '../api';

export const useUsersManager = () => {
  const [users, setUsers] = useState<KeycloakUser[]>([]);
  const [realmRoles, setRealmRoles] = useState<RealmRole[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');

  const refresh = useCallback(async (params: UserSearchParams = {}) => {
    setLoading(true);
    setError(null);
    try {
      setUsers(await fetchUsersApi({ search: params.search ?? search, max: 100 }));
    } catch {
      setError('Failed to load users.');
    } finally {
      setLoading(false);
    }
  }, [search]);

  const loadRealmRoles = useCallback(async () => {
    try {
      setRealmRoles(await fetchRealmRolesApi());
    } catch {
      toast.error('Failed to load realm roles.');
    }
  }, []);

  useEffect(() => { refresh(); loadRealmRoles(); }, [refresh, loadRealmRoles]);

  const createUser = async (form: CreateUserRequest): Promise<string | null> => {
    setLoading(true);
    try {
      const user = await createUserApi(form);
      setUsers(prev => [user, ...prev]);
      toast.success(`User "${user.username}" created`);
      return null;
    } catch (e) {
      const msg = e instanceof ApiError ? e.detail : 'Failed to create user.';
      toast.error(msg);
      return msg;
    } finally {
      setLoading(false);
    }
  };

  const updateUser = async (id: string, form: UpdateUserRequest): Promise<string | null> => {
    setLoading(true);
    try {
      const updated = await updateUserApi(id, form);
      setUsers(prev => prev.map(u => (u.id === updated.id ? { ...u, ...updated } : u)));
      toast.success('User updated');
      return null;
    } catch (e) {
      const msg = e instanceof ApiError ? e.detail : 'Failed to update user.';
      toast.error(msg);
      return msg;
    } finally {
      setLoading(false);
    }
  };

  const deleteUser = async (id: string): Promise<string | null> => {
    setLoading(true);
    try {
      await deleteUserApi(id);
      setUsers(prev => prev.filter(u => u.id !== id));
      toast.success('User deleted');
      return null;
    } catch (e) {
      const msg = e instanceof ApiError ? e.detail : 'Failed to delete user.';
      toast.error(msg);
      return msg;
    } finally {
      setLoading(false);
    }
  };

  const resetPassword = async (id: string, req: ResetPasswordRequest): Promise<string | null> => {
    setLoading(true);
    try {
      await resetPasswordApi(id, req);
      toast.success('Password reset');
      return null;
    } catch (e) {
      const msg = e instanceof ApiError ? e.detail : 'Failed to reset password.';
      toast.error(msg);
      return msg;
    } finally {
      setLoading(false);
    }
  };

  const assignRoles = async (id: string, req: AssignRolesRequest): Promise<string | null> => {
    setLoading(true);
    try {
      await assignRolesApi(id, req);
      setUsers(prev => prev.map(u => (u.id === id ? { ...u, roles: req.roles } : u)));
      toast.success('Roles updated');
      return null;
    } catch (e) {
      const msg = e instanceof ApiError ? e.detail : 'Failed to assign roles.';
      toast.error(msg);
      return msg;
    } finally {
      setLoading(false);
    }
  };

  return {
    users,
    realmRoles,
    loading,
    error,
    search,
    setSearch,
    refresh,
    createUser,
    updateUser,
    deleteUser,
    resetPassword,
    assignRoles,
  };
};
