/**
 * Represents the authenticated user.
 */
export interface User {
  username: string;
}

/**
 * Represents the response from the login endpoint.
 */
export interface LoginResponse {
  token: string;
  username: string;
}
