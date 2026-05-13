export interface KeycloakUser {
  id: string;
  username: string;
  email?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  enabled: boolean;
  emailVerified: boolean;
  createdTimestamp?: number | null;
  roles: string[];
}

export interface RealmRole {
  id: string;
  name: string;
  description?: string | null;
}

export interface CreateUserRequest {
  username: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  enabled: boolean;
  password: string;
  temporaryPassword: boolean;
  roles: string[];
}

export interface UpdateUserRequest {
  email?: string;
  firstName?: string;
  lastName?: string;
  enabled: boolean;
}

export interface ResetPasswordRequest {
  password: string;
  temporary: boolean;
}

export interface AssignRolesRequest {
  roles: string[];
}

export interface UserSearchParams {
  search?: string;
  first?: number;
  max?: number;
  enabled?: boolean;
}
