import { Injectable, signal } from '@angular/core';

export interface StoredUser {
  username: string;
  roles: string[];
}

/**
 * Single source of truth for the persisted auth token + user.
 * Wraps localStorage in signals so the rest of the app reacts to changes.
 * Ports `getStoredAuth` from the React AuthProvider.
 */
@Injectable({ providedIn: 'root' })
export class TokenStore {
  readonly token = signal<string | null>(localStorage.getItem('token'));
  readonly user = signal<StoredUser | null>(this.read());

  set(token: string, user: StoredUser): void {
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify(user));
    this.token.set(token);
    this.user.set(user);
  }

  clear(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.token.set(null);
    this.user.set(null);
  }

  private read(): StoredUser | null {
    const raw = localStorage.getItem('user');
    if (!raw) return null;
    try {
      return JSON.parse(raw) as StoredUser;
    } catch {
      return null;
    }
  }
}
