import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
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
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [fieldErrors, setFieldErrors] = useState<string[]>([]);

  const navigate = useNavigate();

  /**
   * Validates client-side rules before hitting the API,
   * then delegates to authService.register and redirects on success.
   */
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFieldErrors([]);

    if (password !== confirmPassword) {
      setFieldErrors(['Passwords do not match.']);
      return;
    }

    setIsLoading(true);
    try {
      await authService.register({ email, password });
      notificationService.success('Account created! You can now sign in.');
      navigate('/login');
    } catch (err) {
      if (axios.isAxiosError(err)) {
        const data = err.response?.data;
        if (data?.errors && Array.isArray(data.errors)) {
          setFieldErrors(data.errors as string[]);
        } else {
          notificationService.error('Registration failed. Please try again.');
        }
      } else {
        notificationService.error('An unexpected error occurred.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-full items-center justify-center bg-gray-50 dark:bg-slate-950 px-4 py-12 sm:px-6 lg:px-8 transition-colors duration-300">
      <SEO title="Create an Account" description="Register a free Reader account on hcioffi.dev to join the community." />

      <div className="w-full max-w-md space-y-8">
        {/* Header */}
        <div className="text-center">
          <h1 className="mt-6 text-3xl font-bold tracking-tight text-gray-900 dark:text-slate-100">
            Create your account
          </h1>
          <p className="mt-2 text-sm text-gray-500 dark:text-slate-400">
            Already have an account?{' '}
            <Link to="/login" className="font-medium text-indigo-600 dark:text-indigo-400 hover:underline">
              Sign in
            </Link>
          </p>
        </div>

        {/* Error list */}
        {fieldErrors.length > 0 && (
          <div
            role="alert"
            className="rounded-md bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900/50 p-4"
          >
            <ul className="list-disc list-inside space-y-1 text-sm text-red-700 dark:text-red-400">
              {fieldErrors.map((err, i) => (
                <li key={i}>{err}</li>
              ))}
            </ul>
          </div>
        )}

        {/* Form */}
        <form className="mt-8 space-y-4" onSubmit={handleSubmit} noValidate>
          <div>
            <label htmlFor="email" className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">
              Email address
            </label>
            <input
              id="email"
              name="email"
              type="email"
              required
              autoComplete="email"
              className="block w-full rounded-md border-0 py-2.5 px-3 text-gray-900 dark:text-slate-100 bg-white dark:bg-slate-900 ring-1 ring-inset ring-gray-300 dark:ring-slate-700 placeholder:text-gray-400 dark:placeholder:text-gray-500 focus:ring-2 focus:ring-inset focus:ring-indigo-600 dark:focus:ring-indigo-500 sm:text-sm transition-shadow"
              placeholder="you@example.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          </div>

          <div>
            <label htmlFor="password" className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">
              Password
            </label>
            <input
              id="password"
              name="password"
              type="password"
              required
              autoComplete="new-password"
              className="block w-full rounded-md border-0 py-2.5 px-3 text-gray-900 dark:text-slate-100 bg-white dark:bg-slate-900 ring-1 ring-inset ring-gray-300 dark:ring-slate-700 placeholder:text-gray-400 dark:placeholder:text-gray-500 focus:ring-2 focus:ring-inset focus:ring-indigo-600 dark:focus:ring-indigo-500 sm:text-sm transition-shadow"
              placeholder="At least 8 characters"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>

          <div>
            <label htmlFor="confirm-password" className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">
              Confirm password
            </label>
            <input
              id="confirm-password"
              name="confirmPassword"
              type="password"
              required
              autoComplete="new-password"
              className="block w-full rounded-md border-0 py-2.5 px-3 text-gray-900 dark:text-slate-100 bg-white dark:bg-slate-900 ring-1 ring-inset ring-gray-300 dark:ring-slate-700 placeholder:text-gray-400 dark:placeholder:text-gray-500 focus:ring-2 focus:ring-inset focus:ring-indigo-600 dark:focus:ring-indigo-500 sm:text-sm transition-shadow"
              placeholder="Repeat your password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
            />
          </div>

          <button
            type="submit"
            id="btn-register-submit"
            disabled={isLoading}
            className="mt-2 flex w-full justify-center rounded-md bg-indigo-600 dark:bg-indigo-500 px-3 py-2.5 text-sm font-semibold text-white hover:bg-indigo-700 dark:hover:bg-indigo-600 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 disabled:opacity-50 transition-colors"
          >
            {isLoading ? 'Creating account…' : 'Create account'}
          </button>
        </form>

        {/* Terms note */}
        <p className="text-center text-xs text-gray-400 dark:text-slate-600">
          By creating an account, you agree to our terms of service.
          Your account will be assigned the <span className="font-medium">Reader</span> role.
        </p>
      </div>
    </div>
  );
};

export default Register;
