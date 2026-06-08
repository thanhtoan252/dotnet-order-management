import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  AssignRolesRequest,
  CreateUserRequest,
  KeycloakUser,
  RealmRole,
  ResetPasswordRequest,
  UpdateUserRequest,
  UserSearchParams,
} from '../models/user.model';

/**
 * HTTP access for Keycloak user management.
 * Ports `features/user-management/api`; every call carries the `X-Api-Version` header.
 */
@Injectable({ providedIn: 'root' })
export class UsersApi {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/identity/users`;
  private readonly headers = { 'X-Api-Version': '1.0' };

  list(params: UserSearchParams = {}): Observable<KeycloakUser[]> {
    let p = new HttpParams();
    if (params.search) p = p.set('search', params.search);
    if (params.first !== undefined) p = p.set('first', params.first);
    if (params.max !== undefined) p = p.set('max', params.max);
    if (params.enabled !== undefined) p = p.set('enabled', params.enabled ? 'true' : 'false');
    return this.http.get<KeycloakUser[]>(this.base, { headers: this.headers, params: p });
  }

  realmRoles(): Observable<RealmRole[]> {
    return this.http.get<RealmRole[]>(`${this.base}/realm-roles`, { headers: this.headers });
  }

  userRoles(id: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.base}/${id}/roles`, { headers: this.headers });
  }

  create(req: CreateUserRequest): Observable<KeycloakUser> {
    return this.http.post<KeycloakUser>(this.base, req, { headers: this.headers });
  }

  update(id: string, req: UpdateUserRequest): Observable<KeycloakUser> {
    return this.http.put<KeycloakUser>(`${this.base}/${id}`, req, { headers: this.headers });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`, { headers: this.headers });
  }

  resetPassword(id: string, req: ResetPasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/reset-password`, req, {
      headers: this.headers,
    });
  }

  assignRoles(id: string, req: AssignRolesRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/roles`, req, { headers: this.headers });
  }
}
