import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { StoredUser, TokenStore } from './token-store';

interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  username: string;
  roles: string[];
}

/**
 * Signal-based auth facade. Ports the React `AuthProvider` + `useAuth`.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly store = inject(TokenStore);
  private readonly router = inject(Router);

  readonly user = this.store.user;
  readonly isAuthenticated = computed(() => this.user() !== null);
  readonly username = computed(() => this.user()?.username ?? '');
  readonly roles = computed(() => this.user()?.roles ?? []);

  login(username: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiBaseUrl}/auth/login`, { username, password })
      .pipe(
        tap((r) => {
          const stored: StoredUser = { username: r.username, roles: r.roles };
          this.store.set(r.accessToken, stored);
        }),
      );
  }

  logout(): void {
    this.store.clear();
    void this.router.navigate(['/login']);
  }
}
