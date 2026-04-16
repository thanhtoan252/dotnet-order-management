import { createContext, useContext, useState, useCallback, type ReactNode, type FormEvent } from 'react';
import { apiClient } from '../../lib/api';
import { LayoutGrid } from 'lucide-react';

interface AuthContextValue {
  isAuthenticated: boolean;
  username?: string;
  roles: string[];
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue>({
  isAuthenticated: false,
  roles: [],
  logout: () => {},
});

export const useAuth = () => useContext(AuthContext);

interface StoredUser {
  username: string;
  roles: string[];
}

function getStoredAuth(): StoredUser | null {
  const token = localStorage.getItem('token');
  const raw = localStorage.getItem('user');
  if (!token || !raw) return null;
  try {
    return JSON.parse(raw) as StoredUser;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<StoredUser | null>(getStoredAuth);

  const logout = useCallback(() => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
  }, []);

  if (!user) {
    return <LoginForm onSuccess={setUser} />;
  }

  return (
    <AuthContext.Provider value={{
      isAuthenticated: true,
      username: user.username,
      roles: user.roles,
      logout,
    }}>
      {children}
    </AuthContext.Provider>
  );
}

function LoginForm({ onSuccess }: { onSuccess: (user: StoredUser) => void }) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const { data } = await apiClient.post<{
        accessToken: string;
        refreshToken: string;
        username: string;
        roles: string[];
      }>('/auth/login', { username, password });

      const stored: StoredUser = { username: data.username, roles: data.roles };
      localStorage.setItem('token', data.accessToken);
      localStorage.setItem('user', JSON.stringify(stored));
      onSuccess(stored);
    } catch {
      setError('Invalid username or password');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-50 px-4">
      <div className="w-full max-w-sm">
        {/* Brand */}
        <div className="mb-8 flex flex-col items-center gap-3">
          <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-indigo-600 shadow-lg">
            <LayoutGrid className="h-6 w-6 text-white" />
          </div>
          <div className="text-center">
            <h1 className="text-xl font-bold text-slate-900">Order Management</h1>
            <p className="text-sm text-slate-500">Sign in to continue</p>
          </div>
        </div>

        {/* Login card */}
        <form
          onSubmit={handleSubmit}
          className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
        >
          {error && (
            <div className="mb-4 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600">
              {error}
            </div>
          )}

          <div className="space-y-4">
            <div>
              <label htmlFor="username" className="mb-1.5 block text-sm font-medium text-slate-700">
                Username
              </label>
              <input
                id="username"
                type="text"
                required
                autoFocus
                autoComplete="username"
                value={username}
                onChange={e => setUsername(e.target.value)}
                className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/20"
                placeholder="Enter your username"
              />
            </div>

            <div>
              <label htmlFor="password" className="mb-1.5 block text-sm font-medium text-slate-700">
                Password
              </label>
              <input
                id="password"
                type="password"
                required
                autoComplete="current-password"
                value={password}
                onChange={e => setPassword(e.target.value)}
                className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/20"
                placeholder="Enter your password"
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={loading}
            className="mt-6 w-full rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 disabled:opacity-60"
          >
            {loading ? 'Signing in...' : 'Sign in'}
          </button>

          <p className="mt-4 text-center text-xs text-slate-400">
            admin / admin123 &nbsp;&middot;&nbsp; user / user123
          </p>
        </form>
      </div>
    </div>
  );
}
