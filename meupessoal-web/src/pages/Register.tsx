import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation, Trans } from 'react-i18next';
import axios from 'axios';
import authService from '../services/authService';
import notificationService from '../services/notificationService';
import { SEO } from '../components/SEO';

/**
 * Renders the public registration page for new Reader accounts.
 * Handles form validation feedback inline and delegates submission to authService.
 *
 * @returns {JSX.Element} The rendered registration form.
 */
const Register: React.FC = () => {
  const { t } = useTranslation();
  const [userName, setUserName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [fieldErrors, setFieldErrors] = useState<string[]>([]);

  const navigate = useNavigate();

  /**
   * Validates client-side rules before hitting the API.
   * Sends userName, email, and password to the backend.
   */
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFieldErrors([]);

    // Basic validation for name length
    if (userName.trim().length < 3) {
      setFieldErrors([t('auth.register.errorNameLength')]);
      return;
    }

    if (password !== confirmPassword) {
      setFieldErrors([t('auth.register.errorPasswordMatch')]);
      return;
    }

    setIsLoading(true);
    try {
      await authService.register({ userName, email, password });
      notificationService.success(t('auth.register.success'));
      navigate('/check-email');
    } catch (err) {
      if (axios.isAxiosError(err)) {
        const data = err.response?.data;
        if (data?.errors && Array.isArray(data.errors)) {
          setFieldErrors(data.errors as string[]);
        } else {
          notificationService.error(t('auth.register.failed'));
        }
      } else {
        notificationService.error(t('common.error'));
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-full items-center justify-center bg-gray-50 dark:bg-slate-950 px-4 py-12 sm:px-6 lg:px-8 transition-colors duration-300 font-bold">
      <SEO title={t('auth.register.seoTitle')} description={t('auth.register.seoDesc')} />

      <div className="w-full max-w-md space-y-8">
        <div className="text-center">
          <h1 className="mt-6 text-3xl font-bold tracking-tight text-gray-900 dark:text-slate-100">
            {t('auth.register.title')}
          </h1>
          <p className="mt-2 text-sm text-gray-500 dark:text-slate-400">
            {t('auth.register.hasAccount')}{' '}
            <Link to="/login" className="font-medium text-indigo-600 dark:text-indigo-400 hover:underline">
              {t('auth.register.signIn')}
            </Link>
          </p>
        </div>

        {/* Error list */}
        {fieldErrors.length > 0 && (
          <div
            role="alert"
            className="rounded-md bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900/50 p-4"
          >
            <ul className="list-disc list-inside space-y-1 text-sm text-red-700 dark:text-red-400 font-medium">
              {fieldErrors.map((err, i) => (
                <li key={i}>{err}</li>
              ))}
            </ul>
          </div>
        )}

        {/* Form */}
        <form className="mt-8 space-y-4" onSubmit={handleSubmit} noValidate>
          <div>
            <label htmlFor="username" className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-1">
              {t('auth.register.username')}
            </label>
            <input
              id="username"
              name="username"
              type="text"
              required
              className="block w-full rounded-md border-0 py-2.5 px-3 text-gray-900 dark:text-slate-100 bg-white dark:bg-slate-900 ring-1 ring-inset ring-gray-300 dark:ring-slate-700 placeholder:text-gray-400 dark:placeholder:text-gray-500 focus:ring-2 focus:ring-inset focus:ring-indigo-600 dark:focus:ring-indigo-500 sm:text-sm transition-shadow outline-none"
              placeholder={t('auth.register.usernamePlaceholder')}
              value={userName}
              onChange={(e) => setUserName(e.target.value)}
            />
          </div>

          <div>
            <label htmlFor="email" className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-1">
              {t('auth.register.email')}
            </label>
            <input
              id="email"
              name="email"
              type="email"
              required
              autoComplete="email"
              className="block w-full rounded-md border-0 py-2.5 px-3 text-gray-900 dark:text-slate-100 bg-white dark:bg-slate-900 ring-1 ring-inset ring-gray-300 dark:ring-slate-700 placeholder:text-gray-400 dark:placeholder:text-gray-500 focus:ring-2 focus:ring-inset focus:ring-indigo-600 dark:focus:ring-indigo-500 sm:text-sm transition-shadow outline-none"
              placeholder={t('auth.register.emailPlaceholder')}
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          </div>

          <div>
            <label htmlFor="password" className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-1">
              {t('auth.register.password')}
            </label>
            <input
              id="password"
              name="password"
              type="password"
              required
              autoComplete="new-password"
              className="block w-full rounded-md border-0 py-2.5 px-3 text-gray-900 dark:text-slate-100 bg-white dark:bg-slate-900 ring-1 ring-inset ring-gray-300 dark:ring-slate-700 placeholder:text-gray-400 dark:placeholder:text-gray-500 focus:ring-2 focus:ring-inset focus:ring-indigo-600 dark:focus:ring-indigo-500 sm:text-sm transition-shadow outline-none"
              placeholder={t('auth.register.passwordPlaceholder')}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>

          <div>
            <label htmlFor="confirm-password" className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-1">
              {t('auth.register.confirmPassword')}
            </label>
            <input
              id="confirm-password"
              name="confirmPassword"
              type="password"
              required
              autoComplete="new-password"
              className="block w-full rounded-md border-0 py-2.5 px-3 text-gray-900 dark:text-slate-100 bg-white dark:bg-slate-900 ring-1 ring-inset ring-gray-300 dark:ring-slate-700 placeholder:text-gray-400 dark:placeholder:text-gray-500 focus:ring-2 focus:ring-inset focus:ring-indigo-600 dark:focus:ring-indigo-500 sm:text-sm transition-shadow outline-none"
              placeholder={t('auth.register.confirmPasswordPlaceholder')}
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
            />
          </div>

          <button
            type="submit"
            id="btn-register-submit"
            disabled={isLoading}
            className="mt-2 flex w-full justify-center 
            rounded-md bg-indigo-600 dark:bg-indigo-500 px-3 py-2.5 text-sm 
            font-extrabold text-white hover:bg-indigo-700 dark:hover:bg-indigo-600 
            focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 
            focus-visible:outline-indigo-600 disabled:opacity-50 transition-colors cursor-pointer uppercase tracking-wide">
            {isLoading ? t('auth.register.creating') : t('auth.register.button')}
          </button>
        </form>

        {/* Terms note */}
        <p className="text-center text-xs text-gray-400 dark:text-slate-600">
          <Trans i18nKey="auth.register.terms">
            By creating an account, you agree to our terms of service.
            Your account will be assigned the <span className="font-medium">Reader</span> role.
          </Trans>
        </p>
      </div>
    </div>
  );
};

export default Register;
