import axios from 'axios';
import { storage } from '../util/storage';
import i18n from '../i18n/config';

// HC: Logic to determine the API base URL based on the environment mode.
const isDev = import.meta.env.MODE === 'development';

export const API_BASE_URL = import.meta.env.VITE_API_URL || 
    (isDev 
        ? 'http://localhost:25683/api' 
        : '/api');

// Derived logic remains the same to handle root URL transformations.
export const BACKEND_URL = API_BASE_URL.replace(/\/api\/?$/, '');

/**
 * Creates a centralized Axios instance configured with the backend base URL.
 */
const api = axios.create({
    baseURL: API_BASE_URL,
    paramsSerializer: {
        indexes: null 
    }
});

/**
 * Request interceptor injects the JWT token and current language preference.
 * This ensures every outgoing request is authenticated and localized.
 */
api.interceptors.request.use((config) => {
    const token = storage.getToken();
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }

    // Inject the current language for content projection on the backend
    config.headers['Accept-Language'] = i18n.language || 'en';
    
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