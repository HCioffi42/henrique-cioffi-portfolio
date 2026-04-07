import React, { useState, useCallback, useMemo } from 'react';
import { jwtDecode } from 'jwt-decode';
import { storage } from '../util/storage';
import { AuthContext } from './AuthContext';
import type { ReactNode } from 'react';
import type { User } from '../models/Auth';

/**
 * Interface for the decoded JWT payload based on ASP.NET Core Identity claims.
 */
interface JwtPayload {
  sub: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': string | string[];
  exp: number;
}

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
  
  /**
   * Helper to extract user info from a JWT token.
   */
  const getUserFromToken = useCallback((token: string | null): User | null => {
    if (!token) return null;
    try {
      const decoded = jwtDecode<JwtPayload>(token);
      
      // Standard claim names mapped to our User model.
      const roles = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      const primaryRole = Array.isArray(roles) ? roles[0] : roles;

      return {
        id: decoded.sub,
        username: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
        role: primaryRole || 'Reader'
      };
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }, []);

  // Initializes user state by decoding the token.
  const [user, setUser] = useState<User | null>(() => getUserFromToken(token));

  // The initialization flag is set to true as storage reading is synchronous.
  const [isInitialized] = useState(true);

  /**
   * Updates the authentication state and persists credentials to storage.
   * @param newToken The JWT issued by the backend.
   * @param username The display name associated with the user.
   */
  const login = useCallback((newToken: string, username: string) => {
    // Persists session data using the storage utility.
    storage.saveSession(newToken, username);
    
    // Updates React state, which triggers an automatic update of 'isAuthenticated'.
    setToken(newToken);
    setUser(getUserFromToken(newToken));
  }, [getUserFromToken]);

  /**
   * Clears all authentication data from state and persistent storage.
   */
  const logout = useCallback(() => {
    // Removes all credentials from localStorage.
    storage.clearSession();
    
    // Resets React state to null.
    setToken(null);
    setUser(null);
  }, []);

  // Derived properties to prevent unnecessary re-renders.
  const value = useMemo(() => ({
    user, 
    token, 
    isAuthenticated: !!token, 
    isInitialized, 
    login, 
    logout 
  }), [user, token, isInitialized, login, logout]);

  // Prevents rendering until the initial state hydration is complete.
  if (!isInitialized) return null; 

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};
