import axios from 'axios';
import { storage } from '../util/storage';

export const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:25683/api';

// Derive the root backend URL (e.g., http://localhost:25683) from API_BASE_URL
export const BACKEND_URL = API_BASE_URL.replace(/\/api\/?$/, '');

/**
 * Creates a centralized Axios instance configured with the backend base URL.
 */
const api = axios.create({
    baseURL: API_BASE_URL
});

/**
 * Request interceptor injects the JWT token from storage into the Authorization header.
 * This ensures every outgoing request to the API is authenticated if a session exists.
 */
api.interceptors.request.use((config) => {
    const token = storage.getToken();
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

/**
 * Response interceptor monitors for authentication errors.
 * If the server returns a 401 Unauthorized status, it clears the local session 
 * and redirects the user to the login page to prevent unauthorized state drift.
 */
api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            storage.clearSession();
            
            // Forces a redirect to login only if the user is not already there.
            if (!window.location.pathname.includes('/login')) {
                window.location.href = '/login';
            }
        }
        return Promise.reject(error);
    }
);

export default api;