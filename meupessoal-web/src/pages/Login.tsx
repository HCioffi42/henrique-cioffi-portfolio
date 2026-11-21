import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import axios from 'axios';
import { useAuth } from '../context/AuthContext';
import authService from '../services/authService';
import notificationService from '../services/notificationService';

/**
 * Renders the login page for local credential authentication.
 *
 * @returns {JSX.Element} The rendered login page.
 */
const Login: React.FC = () => {
  const { t } = useTranslation();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  /**
   * Handles the credential form submission. On success, issues the JWT and navigates home.
   */
  const handleCredentialSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    try {
      const response = await authService.login(username, password);

      if (response.token && response.username) {
        login(response.token, response.username);
        notificationService.success(t('auth.login.welcome', { username: response.username }));
        navigate('/');
      }
    } catch (err) {
      const message = axios.isAxiosError(err) && err.response?.status === 401
        ? t('auth.login.invalidCredentials')
        : t('auth.login.serverError');
      notificationService.error(message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-full items-center justify-center bg-gray-50 dark:bg-slate-950 px-4 py-12 sm:px-6 lg:px-8 transition-colors duration-300 font-bold">
      <div className="w-full max-w-md space-y-8">

        {/* --- Header --- */}
        <div className="text-center">
          <h2 className="mt-6 text-3xl font-bold tracking-tight text-gray-900 dark:text-slate-100">
            {t('auth.login.title')}
          </h2>
          <p className="mt-2 text-sm text-gray-500 dark:text-slate-400">
            {t('auth.login.noAccount')}{' '}
            <Link to="/register" className="font-medium text-indigo-600 dark:text-indigo-400 hover:underline">
              {t('auth.login.createOne')}
            </Link>
          </p>
        </div>

        {/* --- Credentials Form --- */}
        <form className="mt-8 space-y-4" onSubmit={handleCredentialSubmit}>
          <div className="-space-y-px rounded-md shadow-sm">
            <div>
              <input
                id="username"
                name="username"
                type="text"
                required
                autoComplete="username"
                className="relative block w-full rounded-t-md border-0 py-2.5 px-3 text-gray-900 dark:text-slate-100 bg-white dark:bg-slate-900 ring-1 ring-inset ring-gray-300 dark:ring-slate-700 placeholder:text-gray-400 dark:placeholder:text-gray-500 focus:z-10 focus:ring-2 focus:ring-inset focus:ring-indigo-600 dark:focus:ring-indigo-500 sm:text-sm outline-none"
                placeholder={t('auth.login.username')}
                value={username}
                onChange={(e) => setUsername(e.target.value)}
              />
            </div>
            <div>
              <input
                id="password"
                name="password"
                type="password"
                required
                autoComplete="current-password"
                className="relative block w-full rounded-b-md border-0 py-2.5 px-3 text-gray-900 dark:text-slate-100 bg-white dark:bg-slate-900 ring-1 ring-inset ring-gray-300 dark:ring-slate-700 placeholder:text-gray-400 dark:placeholder:text-gray-500 focus:z-10 focus:ring-2 focus:ring-inset focus:ring-indigo-600 dark:focus:ring-indigo-500 sm:text-sm outline-none"
                placeholder={t('auth.login.password')}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </div>
          </div>

          <button
            type="submit"
            id="btn-login-submit"
            disabled={isLoading}
            className="group relative flex w-full justify-center rounded-md bg-indigo-600 dark:bg-indigo-500 px-3 py-2.5 text-sm font-semibold text-white hover:bg-indigo-700 dark:hover:bg-indigo-600 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 disabled:opacity-50 transition-colors cursor-pointer"
          >
            {isLoading ? t('auth.login.signingIn') : t('auth.login.signIn')}
          </button>
        </form>

      </div>
    </div>
  );
};

export default Login;
