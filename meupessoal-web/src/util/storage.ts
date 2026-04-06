const TOKEN_KEY = 'token';
const USER_KEY = 'username';

/**
 * Utility for managing persistent authentication data in the browser's localStorage.
 * Centralizes access to sensitive session keys to ensure consistency.
 */
export const storage = {
  // Retrieves the stored JWT token.
  getToken: () => localStorage.getItem(TOKEN_KEY),

  // Retrieves the stored display name (username).
  getUsername: () => localStorage.getItem(USER_KEY),

  /**
   * Persists both the token and the username to the storage.
   * This ensures the application can recover the user's identity after a page refresh.
   */
  saveSession: (token: string, username: string) => {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(USER_KEY, username);
  },

  // Removes all authentication-related keys from the storage.
  clearSession: () => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }
};