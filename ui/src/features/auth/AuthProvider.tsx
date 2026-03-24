import { createContext, useContext, useEffect, useRef, useState, type ReactNode } from 'react';
import keycloak from '../../lib/keycloak';

interface AuthContextValue {
  isLoading: boolean;
  isAuthenticated: boolean;
  username?: string;
  roles: string[];
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue>({
  isLoading: true,
  isAuthenticated: false,
  roles: [],
  logout: () => {},
});

export const useAuth = () => useContext(AuthContext);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isLoading, setIsLoading] = useState(true);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const initialized = useRef(false);

  useEffect(() => {
    if (initialized.current) return;
    initialized.current = true;

    keycloak
      .init({ onLoad: 'login-required', checkLoginIframe: false, pkceMethod: 'S256' })
      .then(authenticated => {
        setIsAuthenticated(authenticated);
        setIsLoading(false);
      })
      .catch(() => setIsLoading(false));
  }, []);

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center bg-slate-50">
        <div className="flex flex-col items-center gap-3">
          <div className="w-8 h-8 border-2 border-indigo-600 border-t-transparent rounded-full animate-spin" />
          <p className="text-sm text-slate-500">Authenticating...</p>
        </div>
      </div>
    );
  }

  const roles: string[] = (keycloak.tokenParsed?.roles as string[]) ?? [];

  return (
    <AuthContext.Provider value={{
      isLoading,
      isAuthenticated,
      username: keycloak.tokenParsed?.preferred_username as string | undefined,
      roles,
      logout: () => keycloak.logout({ redirectUri: window.location.origin }),
    }}>
      {children}
    </AuthContext.Provider>
  );
}
