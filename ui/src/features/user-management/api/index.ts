import { apiClient } from '../../../lib/api';
import type {
  KeycloakUser,
  RealmRole,
  CreateUserRequest,
  UpdateUserRequest,
  ResetPasswordRequest,
  AssignRolesRequest,
  UserSearchParams,
} from '../types';

const BASE = '/identity/users';

export const fetchUsersApi = async (params: UserSearchParams = {}): Promise<KeycloakUser[]> => {
  const query: Record<string, string | number> = {};
  if (params.search) query.search = params.search;
  if (params.first !== undefined) query.first = params.first;
  if (params.max !== undefined) query.max = params.max;
  if (params.enabled !== undefined) query.enabled = params.enabled ? 'true' : 'false';

  const { data } = await apiClient.get<KeycloakUser[]>(BASE, { params: query });
  return data;
};

export const fetchUserByIdApi = async (id: string): Promise<KeycloakUser> => {
  const { data } = await apiClient.get<KeycloakUser>(`${BASE}/${id}`);
  return data;
};

export const fetchRealmRolesApi = async (): Promise<RealmRole[]> => {
  const { data } = await apiClient.get<RealmRole[]>(`${BASE}/realm-roles`);
  return data;
};

export const fetchUserRolesApi = async (id: string): Promise<string[]> => {
  const { data } = await apiClient.get<string[]>(`${BASE}/${id}/roles`);
  return data;
};

export const createUserApi = async (req: CreateUserRequest): Promise<KeycloakUser> => {
  const { data } = await apiClient.post<KeycloakUser>(BASE, req);
  return data;
};

export const updateUserApi = async (id: string, req: UpdateUserRequest): Promise<KeycloakUser> => {
  const { data } = await apiClient.put<KeycloakUser>(`${BASE}/${id}`, req);
  return data;
};

export const deleteUserApi = async (id: string): Promise<void> => {
  await apiClient.delete(`${BASE}/${id}`);
};

export const resetPasswordApi = async (id: string, req: ResetPasswordRequest): Promise<void> => {
  await apiClient.post(`${BASE}/${id}/reset-password`, req);
};

export const assignRolesApi = async (id: string, req: AssignRolesRequest): Promise<void> => {
  await apiClient.put(`${BASE}/${id}/roles`, req);
};
