import { inject, Signal } from '@angular/core';
import { injectMutation, injectQuery, QueryClient } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';
import { ApiError } from '../../../core/http/api-error';
import { NotificationService } from '../../../core/notification.service';
import {
  AssignRolesRequest,
  CreateUserRequest,
  ResetPasswordRequest,
  UpdateUserRequest,
} from '../models/user.model';
import { UsersApi } from './users.api';

export const userKeys = {
  all: ['users'] as const,
  list: (search: string) => ['users', { search }] as const,
  realmRoles: ['realm-roles'] as const,
  userRoles: (id: string) => ['user-roles', id] as const,
};

/** Reactive list query: re-runs whenever the applied `search` signal changes. */
export function injectUsersQuery(search: Signal<string>) {
  const api = inject(UsersApi);
  return injectQuery(() => ({
    queryKey: userKeys.list(search()),
    queryFn: () => firstValueFrom(api.list({ search: search() || undefined, max: 100 })),
  }));
}

export function injectRealmRolesQuery() {
  const api = inject(UsersApi);
  return injectQuery(() => ({
    queryKey: userKeys.realmRoles,
    queryFn: () => firstValueFrom(api.realmRoles()),
    staleTime: 5 * 60_000,
  }));
}

export function injectUserRolesQuery(userId: Signal<string>) {
  const api = inject(UsersApi);
  return injectQuery(() => ({
    queryKey: userKeys.userRoles(userId()),
    queryFn: () => firstValueFrom(api.userRoles(userId())),
  }));
}

export function injectCreateUser() {
  const api = inject(UsersApi);
  const qc = inject(QueryClient);
  const notify = inject(NotificationService);
  return injectMutation(() => ({
    mutationFn: (dto: CreateUserRequest) => firstValueFrom(api.create(dto)),
    onSuccess: (u) => {
      qc.invalidateQueries({ queryKey: userKeys.all });
      notify.success(`User "${u.username}" created`);
    },
    onError: (e: ApiError) => notify.error(e.detail),
  }));
}

export function injectUpdateUser() {
  const api = inject(UsersApi);
  const qc = inject(QueryClient);
  const notify = inject(NotificationService);
  return injectMutation(() => ({
    mutationFn: (vars: { id: string; dto: UpdateUserRequest }) =>
      firstValueFrom(api.update(vars.id, vars.dto)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: userKeys.all });
      notify.success('User updated');
    },
    onError: (e: ApiError) => notify.error(e.detail),
  }));
}

export function injectDeleteUser() {
  const api = inject(UsersApi);
  const qc = inject(QueryClient);
  const notify = inject(NotificationService);
  return injectMutation(() => ({
    mutationFn: (id: string) => firstValueFrom(api.delete(id)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: userKeys.all });
      notify.success('User deleted');
    },
    onError: (e: ApiError) => notify.error(e.detail),
  }));
}

export function injectResetPassword() {
  const api = inject(UsersApi);
  const notify = inject(NotificationService);
  return injectMutation(() => ({
    mutationFn: (vars: { id: string; dto: ResetPasswordRequest }) =>
      firstValueFrom(api.resetPassword(vars.id, vars.dto)),
    onSuccess: () => notify.success('Password reset'),
    onError: (e: ApiError) => notify.error(e.detail),
  }));
}

export function injectAssignRoles() {
  const api = inject(UsersApi);
  const qc = inject(QueryClient);
  const notify = inject(NotificationService);
  return injectMutation(() => ({
    mutationFn: (vars: { id: string; dto: AssignRolesRequest }) =>
      firstValueFrom(api.assignRoles(vars.id, vars.dto)),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: userKeys.all });
      qc.invalidateQueries({ queryKey: userKeys.userRoles(vars.id) });
      notify.success('Roles updated');
    },
    onError: (e: ApiError) => notify.error(e.detail),
  }));
}
