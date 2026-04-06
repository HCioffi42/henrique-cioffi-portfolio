/**
 * Represents an authenticated user.
 */
export interface User {
  username: string;
}

/**
 * Represents the server response from the local login endpoint.
 * When requiresTwoFactor is true, token will be null and the UI must show the 2FA form.
 */
export interface LoginResponse {
  token: string | null;
  username: string | null;
  requiresTwoFactor: boolean;
}

/**
 * Represents the request body for the register endpoint.
 */
export interface RegisterRequest {
  email: string;
  password: string;
}

/**
 * Represents the request body for the 2FA verify endpoint.
 */
export interface VerifyTwoFactorRequest {
  username: string;
  code: string;
}

/**
 * Represents the server response after a successful 2FA verification.
 */
export interface VerifyTwoFactorResponse {
  token: string;
}
