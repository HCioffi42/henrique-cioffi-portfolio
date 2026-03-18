import React, { createContext, useContext, useState, useEffect } from 'react';
import { storage } from '../util/storage';
import type { ReactNode } from 'react';
import type { User } from '../models/Auth';

interface AuthContextType {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isInitialized: boolean;
  login: (token: string, username: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

/**
 * Provider component for the authentication context.
 * Manages user state and persists the JWT token in localStorage.
 * 
 * @param {ReactNode} children The children components to be wrapped.
 * @returns {JSX.Element} The provider element.
 */
export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isInitialized, setIsInitialized] = useState(false);

  useEffect(() => {
    const savedToken = storage.getToken();
    const savedUsername = storage.getUsername();

    if (savedToken && savedUsername) {
      setToken(savedToken);
      setUser({ username: savedUsername });
    }
    setIsInitialized(true);
  }, []);

  const login = (newToken: string, username: string) => {
    storage.saveSession(newToken, username);
    setToken(newToken);
    setUser({ username });
  };

  const logout = () => {
    storage.clearSession();
    setToken(null);
    setUser(null);
  };

  // Avoids flashing protected content while recovering the session
  if (!isInitialized) return null; 

  return (
  <AuthContext.Provider 
    value={{ 
      user, 
      token, 
      isAuthenticated: !!token, 
      isInitialized, 
      login, 
      logout 
    }}>
    {children}
  </AuthContext.Provider>
);
};

/**
 * Custom hook to consume the authentication context.
 * 
 * @returns {AuthContextType} The authentication state and actions.
 * @throws {Error} If used outside of an AuthProvider.
 */
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
