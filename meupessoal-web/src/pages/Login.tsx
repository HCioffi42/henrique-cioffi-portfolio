import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import axios from 'axios';
import { useAuth } from '../context/AuthContext';
import authService from '../services/authService';
import notificationService from '../services/notificationService';

type LoginView = 'credentials' | 'twoFactor';

/**
 * Renders the login page with support for local credential login,
 * a 2FA verification step (conditionally rendered), and external OAuth2 provider buttons.
 * The view transitions from 'credentials' to 'twoFactor' when the server signals 2FA is required.
 *
 * @returns {JSX.Element} The rendered login page.
 */
const Login: React.FC = () => {
  const [view, setView] = useState<LoginView>('credentials');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [totpCode, setTotpCode] = useState('');
  const [pendingUsername, setPendingUsername] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  /**
   * Handles the credential form submission. On success, either logs the user in directly
   * or transitions to the 2FA view depending on the server response.
   */
  const handleCredentialSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    try {
      const response = await authService.login(username, password);

      if (response.requiresTwoFactor && response.username) {
        // The server requires a second factor — store pending username and switch view.
        setPendingUsername(response.username);
        setView('twoFactor');
        notificationService.success('Enter your authenticator code to continue.');
        return;
      }

      if (response.token && response.username) {
        login(response.token, response.username);
        notificationService.success(`Welcome back, ${response.username}!`);
        navigate('/');
      }
    } catch (err) {
      const message = axios.isAxiosError(err) && err.response?.status === 401
        ? 'Invalid username or password.'
        : 'Unable to connect to the server. Please try again later.';
      notificationService.error(message);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Handles the 2FA code submission. On success, issues the JWT and navigates home.
   */
  const handleTotpSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    try {
      const response = await authService.verifyTwoFactor({
        username: pendingUsername,
        code: totpCode,
      });
      login(response.token, pendingUsername);
      notificationService.success(`Welcome back, ${pendingUsername}!`);
      navigate('/');
    } catch (err) {
      const message = axios.isAxiosError(err) && err.response?.status === 401
        ? 'Invalid or expired verification code.'
        : 'Something went wrong. Please try again.';
      notificationService.error(message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-full items-center justify-center bg-gray-50 dark:bg-slate-950 px-4 py-12 sm:px-6 lg:px-8 transition-colors duration-300">
      <div className="w-full max-w-md space-y-8">

        {/* --- Header --- */}
        <div className="text-center">
          <h2 className="mt-6 text-3xl font-bold tracking-tight text-gray-900 dark:text-slate-100">
            {view === 'twoFactor' ? 'Two-Factor Verification' : 'Sign in to your account'}
          </h2>
          {view === 'credentials' && (
            <p className="mt-2 text-sm text-gray-500 dark:text-slate-400">
              Don&apos;t have an account?{' '}
              <Link to="/register" className="font-medium text-indigo-600 dark:text-indigo-400 hover:underline">
                Create one
              </Link>
            </p>
          )}
        </div>

        {/* --- Credentials Form --- */}
        {view === 'credentials' && (
          <>
            <form className="mt-8 space-y-4" onSubmit={handleCredentialSubmit}>
              <div className="-space-y-px rounded-md shadow-sm">
                <div>
                  <input
                    id="username"
                    name="username"
                    type="text"
                    required
                    autoComplete="username"
                    className="relative block w-full rounded-t-md border-0 py-2.5 px-3 text-gray-900 dark:text-slate-100 bg-white dark:bg-slate-900 ring-1 ring-inset ring-gray-300 dark:ring-slate-700 placeholder:text-gray-400 dark:placeholder:text-gray-500 focus:z-10 focus:ring-2 focus:ring-inset focus:ring-indigo-600 dark:focus:ring-indigo-500 sm:text-sm"
                    placeholder="Username or email"
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
                    className="relative block w-full rounded-b-md border-0 py-2.5 px-3 text-gray-900 dark:text-slate-100 bg-white dark:bg-slate-900 ring-1 ring-inset ring-gray-300 dark:ring-slate-700 placeholder:text-gray-400 dark:placeholder:text-gray-500 focus:z-10 focus:ring-2 focus:ring-inset focus:ring-indigo-600 dark:focus:ring-indigo-500 sm:text-sm"
                    placeholder="Password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                  />
                </div>
              </div>

              <button
                type="submit"
                id="btn-login-submit"
                disabled={isLoading}
                className="group relative flex w-full justify-center rounded-md bg-indigo-600 dark:bg-indigo-500 px-3 py-2.5 text-sm font-semibold text-white hover:bg-indigo-700 dark:hover:bg-indigo-600 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 disabled:opacity-50 transition-colors"
              >
                {isLoading ? 'Signing in…' : 'Sign in'}
              </button>
            </form>

            {/* --- OAuth Divider --- */}
            {/* HC: Social Login section hidden for now. 
                Uncomment this block when OAuth2 endpoints and redirect URIs are fully configured in the backend.
            */}
            {/* <div className="relative my-6">
              <div className="absolute inset-0 flex items-center" aria-hidden="true">
                <div className="w-full border-t border-gray-200 dark:border-slate-700" />
              </div>
              <div className="relative flex justify-center text-sm">
                <span className="bg-gray-50 dark:bg-slate-950 px-3 text-gray-400 dark:text-slate-500">
                  Or continue with
                </span>
              </div>
            </div>
            {/* --- OAuth Buttons --- */}
            {/*<div className="grid grid-cols-2 gap-3">
              <button
                type="button"
                onClick={() => authService.initiateExternalLogin('GitHub')}
                className="flex w-full items-center justify-center gap-2 rounded-md border border-gray-300 dark:border-slate-700 bg-white dark:bg-slate-900 px-4 py-2.5 text-sm font-medium text-gray-700 dark:text-slate-200 hover:bg-gray-50 transition-colors"
              >
                GitHub
              </button>
              <button
                type="button"
                onClick={() => authService.initiateExternalLogin('Google')}
                className="flex w-full items-center justify-center gap-2 rounded-md border border-gray-300 dark:border-slate-700 bg-white dark:bg-slate-900 px-4 py-2.5 text-sm font-medium text-gray-700 dark:text-slate-200 hover:bg-gray-50 transition-colors"
              >
                Google
              </button>
            </div>
            */}
          </>
        )}

        {/* --- 2FA Form --- */}
        {view === 'twoFactor' && (
          <form className="mt-8 space-y-4" onSubmit={handleTotpSubmit}>
            <div className="rounded-md bg-indigo-50 dark:bg-indigo-950/30 p-4 text-sm text-indigo-700 dark:text-indigo-300 border border-indigo-100 dark:border-indigo-900/50">
              Open your authenticator app and enter the 6-digit code for{' '}
              <span className="font-semibold">{pendingUsername}</span>.
            </div>
            <div>
              <input
                id="totp-code"
                name="code"
                type="text"
                inputMode="numeric"
                pattern="\d{6}"
                maxLength={6}
                required
                autoComplete="one-time-code"
                className="block w-full rounded-md border-0 py-2.5 px-3 text-center text-2xl tracking-[0.5em] font-mono text-gray-900 dark:text-slate-100 bg-white dark:bg-slate-900 ring-1 ring-inset ring-gray-300 dark:ring-slate-700 placeholder:text-gray-300 dark:placeholder:text-slate-600 focus:ring-2 focus:ring-inset focus:ring-indigo-600 dark:focus:ring-indigo-500"
                placeholder="––––––"
                value={totpCode}
                onChange={(e) => setTotpCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
              />
            </div>
            <button
              type="submit"
              id="btn-verify-2fa"
              disabled={isLoading || totpCode.length < 6}
              className="flex w-full justify-center rounded-md bg-indigo-600 dark:bg-indigo-500 px-3 py-2.5 text-sm font-semibold text-white hover:bg-indigo-700 dark:hover:bg-indigo-600 disabled:opacity-50 transition-colors"
            >
              {isLoading ? 'Verifying…' : 'Verify Code'}
            </button>
            <button
              type="button"
              onClick={() => { setView('credentials'); setTotpCode(''); }}
              className="flex w-full justify-center text-sm text-gray-500 dark:text-slate-400 hover:text-gray-700 dark:hover:text-slate-200 transition-colors"
            >
              ← Back to sign in
            </button>
          </form>
        )}

      </div>
    </div>
  );
};

export default Login;
