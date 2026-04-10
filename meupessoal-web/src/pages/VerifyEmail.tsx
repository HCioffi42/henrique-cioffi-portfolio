import { useEffect, useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { CheckCircle, XCircle, Loader2, LogIn, ArrowLeft } from 'lucide-react';
import authService from '../services/authService';

/**
 * Page to handle account email verification.
 * Extracts userId and token from URL and calls the backend confirmation endpoint.
 */
export default function VerifyEmail() {
    const [searchParams] = useSearchParams();
    const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
    const [errorMessage, setErrorMessage] = useState('');

    const userId = searchParams.get('userId');
    const token = searchParams.get('token');

    useEffect(() => {
        const verify = async () => {
            if (!userId || !token) {
                setStatus('error');
                setErrorMessage('Verification parameters are missing.');
                return;
            }

            try {
                await authService.confirmEmail(userId, token);
                setStatus('success');
            } catch (error: any) {
                setStatus('error');
                const errors = error.response?.data?.errors;
                const message = Array.isArray(errors) ? errors[0] : (error.response?.data?.message || 'Invalid or expired verification link.');
                setErrorMessage(message);
            }
        };

        verify();
    }, [userId, token]);

    return (
        <div className="flex flex-col items-center justify-center min-h-[60vh] px-6 text-center">
            <div className="max-w-md w-full bg-white dark:bg-slate-900 p-8 rounded-2xl shadow-sm border border-gray-100 dark:border-slate-800 transition-colors">
                {status === 'loading' && (
                    <div className="flex flex-col items-center gap-4">
                        <Loader2 className="w-12 h-12 text-indigo-600 animate-spin" />
                        <h1 className="text-xl font-bold">Verifying your email...</h1>
                        <p className="text-gray-500 dark:text-gray-400">This will only take a second.</p>
                    </div>
                )}

                {status === 'success' && (
                    <div className="flex flex-col items-center gap-4">
                        <CheckCircle className="w-16 h-16 text-emerald-500" />
                        <h1 className="text-2xl font-bold">Email Verified!</h1>
                        <p className="text-gray-600 dark:text-gray-300">
                            Thank you for verifying your email. Your account is now fully active.
                        </p>
                        <div className="mt-4 flex flex-col sm:flex-row gap-3 w-full justify-center">
                            <Link 
                                to="/login" 
                                className="inline-flex items-center justify-center gap-2 bg-indigo-600 text-white px-6 py-2.5 rounded-lg font-semibold hover:bg-indigo-700 transition-all shadow-sm"
                            >
                                <LogIn className="w-4 h-4" />
                                Login Now
                            </Link>
                            <Link 
                                to="/" 
                                className="inline-flex items-center justify-center gap-2 text-gray-600 dark:text-gray-400 font-semibold hover:underline"
                            >
                                <ArrowLeft className="w-4 h-4" />
                                Back to Home
                            </Link>
                        </div>
                    </div>
                )}

                {status === 'error' && (
                    <div className="flex flex-col items-center gap-4">
                        <XCircle className="w-16 h-16 text-red-500" />
                        <h1 className="text-2xl font-bold">Verification Failed</h1>
                        <p className="text-gray-600 dark:text-gray-300">
                            {errorMessage}
                        </p>
                        <Link 
                            to="/" 
                            className="mt-4 inline-flex items-center gap-2 text-indigo-600 dark:text-indigo-400 font-semibold hover:underline"
                        >
                            <ArrowLeft className="w-4 h-4" />
                            Back to Home
                        </Link>
                    </div>
                )}
            </div>
        </div>
    );
}
