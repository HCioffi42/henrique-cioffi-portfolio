/**
 * Represents an authenticated user.
 */
export interface User {
  id: string;
  username: string;
  role: string;
}

/**
 * Represents the server response from the local login endpoint.
 */
export interface LoginResponse {
  token: string | null;
  username: string | null;
}

/**
 * Represents the request body for the register endpoint.
 */
export interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
}
