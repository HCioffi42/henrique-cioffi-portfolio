import React from 'react';
import { Link } from 'react-router-dom';
import { SEO } from '../components/SEO';
import { Mail, ArrowLeft, CheckCircle } from 'lucide-react';

/**
 * Renders a post-registration landing page instructing the user to check their email.
 */
const CheckEmail: React.FC = () => {
  return (
    <div className="flex min-h-full items-center justify-center bg-gray-50 dark:bg-slate-950 px-4 py-12 sm:px-6 lg:px-8 transition-colors duration-300">
      <SEO title="Check Your Email" description="Your registration was successful. Please check your inbox to verify your email." />

      <div className="w-full max-w-md space-y-8 text-center">
        <div>
          <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-indigo-100 dark:bg-indigo-900/30 text-indigo-600 dark:text-indigo-400">
            <Mail size={32} />
          </div>
          <h1 className="mt-6 text-3xl font-bold tracking-tight text-gray-900 dark:text-slate-100">
            Check your email
          </h1>
          <p className="mt-4 text-base text-gray-600 dark:text-slate-400">
            Success! We've sent a verification link to your inbox. 
            Please click the link in that email to confirm your account and start using the platform.
          </p>
        </div>

        <div className="rounded-lg bg-yellow-50 dark:bg-yellow-900/10 border border-yellow-100 dark:border-yellow-900/20 p-4">
          <div className="flex">
            <div className="flex-shrink-0">
              <CheckCircle className="h-5 w-5 text-yellow-600 dark:text-yellow-500" aria-hidden="true" />
            </div>
            <div className="ml-3">
              <h3 className="text-sm font-medium text-yellow-800 dark:text-yellow-400 text-left">Didn't receive the email?</h3>
              <div className="mt-2 text-sm text-yellow-700 dark:text-yellow-500 text-left">
                <p>Check your <strong>spam folder</strong> or wait a few minutes before trying again.</p>
              </div>
            </div>
          </div>
        </div>

        <div className="flex items-center justify-center">
          <Link
            to="/login"
            className="flex items-center gap-2 text-sm font-medium text-indigo-600 dark:text-indigo-400 hover:underline"
          >
            <ArrowLeft size={16} />
            Back to Sign In
          </Link>
        </div>
      </div>
    </div>
  );
};

export default CheckEmail;
