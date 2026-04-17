import { createContext } from 'react';

export interface AuthContextValue {
  isAuthenticated: boolean;
  username?: string;
  roles: string[];
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue>({
  isAuthenticated: false,
  roles: [],
  logout: () => {},
});
