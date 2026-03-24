import { createContext, useContext } from 'react';
import type { User } from '../models/Auth';

/**
 * Interface defining the shape of the authentication context.
 */
export interface AuthContextType {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isInitialized: boolean;
  login: (token: string, username: string) => void;
  logout: () => void;
}

/**
 * The raw authentication context.
 * Exported here to be used by the Provider, but kept in a .ts file to satisfy Fast Refresh rules.
 */
export const AuthContext = createContext<AuthContextType | undefined>(undefined);

/**
 * Custom hook to consume the authentication context.
 * * @returns {AuthContextType} The authentication state and actions.
 * @throws {Error} If used outside of an AuthProvider.
 */
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};