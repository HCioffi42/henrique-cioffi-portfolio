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
  // Initializes token state by reading from persistent storage.
  const [token, setToken] = useState<string | null>(() => storage.getToken());
  
  // Initializes user state by checking for a saved username in storage.
  const [user, setUser] = useState<User | null>(() => {
    const savedUsername = storage.getUsername();
    return savedUsername ? { username: savedUsername } : null;
  });

  // The initialization flag is set to true as storage reading is synchronous.
  const [isInitialized] = useState(true);

  /**
   * Updates the authentication state and persists credentials to storage.
   * @param newToken The JWT issued by the backend.
   * @param username The display name associated with the user.
   */
  const login = (newToken: string, username: string) => {
    // Persists session data using the storage utility.
    storage.saveSession(newToken, username);
    
    // Updates React state, which triggers an automatic update of 'isAuthenticated'.
    setToken(newToken);
    setUser({ username });
  };

  /**
   * Clears all authentication data from state and persistent storage.
   */
  const logout = () => {
    // Removes all credentials from localStorage.
    storage.clearSession();
    
    // Resets React state to null.
    setToken(null);
    setUser(null);
  };

  // Prevents rendering until the initial state hydration is complete.
  if (!isInitialized) return null; 

  return (
    <AuthContext.Provider value={{ 
      user, 
      token, 
      isAuthenticated: !!token, // Derived property: true if token is not null.
      isInitialized, 
      login, 
      logout 
    }}>
      {children}
    </AuthContext.Provider>
  );
};