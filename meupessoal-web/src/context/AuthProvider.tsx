import React, { useState } from 'react';
import { storage } from '../util/storage';
import { AuthContext } from './AuthContext';
import type { ReactNode } from 'react';
import type { User } from '../models/Auth';

/**
 * Provider component for the authentication context.
 * Manages user state and persists the JWT token in localStorage.
 * 
 * @param {ReactNode} children The children components to be wrapped.
 * @returns {JSX.Element} The provider element.
 */
export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  // Using lazy initialization to read from storage synchronously during the first render.
  // This avoids cascading renders and satisfies the 'no-set-state-in-effect' lint rule.
  const [token, setToken] = useState<string | null>(() => storage.getToken());
  
  const [user, setUser] = useState<User | null>(() => {
    const savedUsername = storage.getUsername();
    return savedUsername ? { username: savedUsername } : null;
  });

  // Since storage reading is now synchronous, the context is initialized immediately.
  const [isInitialized] = useState(true);

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

  // Avoids flashing protected content while recovering the session.
  // In this synchronous version, isInitialized is true by default.
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