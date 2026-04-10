import api from './api';
import type {
  LoginResponse,
  RegisterRequest,
  VerifyTwoFactorRequest,
  VerifyTwoFactorResponse,
} from '../models/Auth';

/**
 * Service for handling all authentication API calls,
 * including local login, registration, 2FA verification, and external OAuth initiation.
 */
const authService = {
  /**
   * Sends a local login request to the API.
   * The response may signal that a 2FA code is required before a token is issued.
   *
   * @param {string} username The username or email address.
   * @param {string} password The user's password.
   * @returns {Promise<LoginResponse>} The login response with token or requiresTwoFactor flag.
   */
  login: async (username: string, password: string): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('/auth/login', { username, password });
    return response.data;
  },

  /**
   * Sends a registration request for a new Reader account.
   *
   * @param {RegisterRequest} request The registration payload.
   * @returns {Promise<void>} Resolves on success; throws on validation/conflict errors.
   */
  register: async (request: RegisterRequest): Promise<void> => {
    await api.post('/auth/register', request);
  },

  /**
   * Verifies a TOTP code after the login step signals 2FA is required.
   *
   * @param {VerifyTwoFactorRequest} request The username and 6-digit code.
   * @returns {Promise<VerifyTwoFactorResponse>} The JWT token on success.
   */
  verifyTwoFactor: async (request: VerifyTwoFactorRequest): Promise<VerifyTwoFactorResponse> => {
    const response = await api.post<VerifyTwoFactorResponse>('/auth/verify-2fa', request);
    return response.data;
  },

  /**
   * Initiates an external OAuth2 login by redirecting the browser window to the
   * backend challenge endpoint for the specified provider.
   *
   * @param {'Google' | 'GitHub'} provider The OAuth2 provider name.
   */
  initiateExternalLogin: (provider: 'Google' | 'GitHub'): void => {
    const apiBase = import.meta.env.VITE_API_URL ?? '';
    window.location.href = `${apiBase}/api/auth/external-login?provider=${provider}`;
  },

  /**
   * Confirms a user's email address using the provided user ID and token.
   *
   * @param {string} userId The user's ID.
   * @param {string} token The verification token.
   * @returns {Promise<void>} Resolves on success.
   */
  confirmEmail: async (userId: string, token: string): Promise<void> => {
    await api.get('/auth/confirm-email', { params: { userId, token } });
  },
};

export default authService;
