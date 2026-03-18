import api from './api';
import type { LoginResponse } from '../models/Auth';

/**
 * Service for handling authentication API calls.
 */
const authService = {
  /**
   * Sends a login request to the API.
   * 
   * @param {string} username The username to authenticate.
   * @param {string} password The password to authenticate.
   * @returns {Promise<LoginResponse>} The login response containing the token and username.
   */
  login: async (username: string, password: string): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('/auth/login', { username, password });
    return response.data;
  },
};

export default authService;
