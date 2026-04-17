import React from 'react';
import { Link } from 'react-router-dom';
import { SEO } from '../components/SEO';
import { UserMinus, Home } from 'lucide-react';

/**
 * Landing page shown after a user successfully unsubscribes from the newsletter.
 */
const UnsubscribeSuccess: React.FC = () => {
  return (
    <div className="flex min-h-full items-center justify-center bg-gray-50 dark:bg-slate-950 px-4 py-12 sm:px-6 lg:px-8 transition-colors duration-300">
      <SEO title="Unsubscribed Successfully" description="You have been unsubscribed from our newsletter." />

      <div className="w-full max-w-md space-y-8 text-center">
        <div>
          <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400">
            <UserMinus size={32} />
          </div>
          <h1 className="mt-6 text-3xl font-bold tracking-tight text-gray-900 dark:text-slate-100">
            Unsubscribed
          </h1>
          <p className="mt-4 text-base text-gray-600 dark:text-slate-400">
            You've been successfully removed from our mailing list. 
            We're sorry to see you go, but we respect your inbox!
          </p>
          <p className="mt-2 text-sm text-gray-500 dark:text-slate-500">
            If this was a mistake, you can always subscribe again from the homepage.
          </p>
        </div>

        <div className="flex items-center justify-center">
          <Link
            to="/"
            className="inline-flex items-center gap-2 rounded-md bg-indigo-600 dark:bg-indigo-500 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-700 dark:hover:bg-indigo-600 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 transition-colors"
          >
            <Home size={16} />
            Back to Homepage
          </Link>
        </div>
      </div>
    </div>
  );
};

export default UnsubscribeSuccess;
