import { useEffect, useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { CheckCircle, XCircle, Loader2, ArrowLeft } from 'lucide-react';
import { newsletterService } from '../services/newsletterService';

/**
 * Page to handle newsletter subscription confirmation.
 * Extracts email and token from URL and calls the backend confirmation endpoint.
 */
export default function ConfirmNewsletter() {
    const [searchParams] = useSearchParams();
    const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
    const [errorMessage, setErrorMessage] = useState('');

    const email = searchParams.get('email');
    const token = searchParams.get('token');

    useEffect(() => {
        const confirm = async () => {
            if (!email || !token) {
                setStatus('error');
                setErrorMessage('Missing confirmation details.');
                return;
            }

            try {
                await newsletterService.confirmSubscription(email, token);
                setStatus('success');
            } catch (error: any) {
                setStatus('error');
                const message = error.response?.data?.message || 'Invalid or expired confirmation link.';
                setErrorMessage(message);
            }
        };

        confirm();
    }, [email, token]);

    return (
        <div className="flex flex-col items-center justify-center min-h-[60vh] px-6 text-center">
            <div className="max-w-md w-full bg-white dark:bg-slate-900 p-8 rounded-2xl shadow-sm border border-gray-100 dark:border-slate-800 transition-colors">
                {status === 'loading' && (
                    <div className="flex flex-col items-center gap-4">
                        <Loader2 className="w-12 h-12 text-indigo-600 animate-spin" />
                        <h1 className="text-xl font-bold">Confirming your subscription...</h1>
                        <p className="text-gray-500 dark:text-gray-400">Please wait a moment.</p>
                    </div>
                )}

                {status === 'success' && (
                    <div className="flex flex-col items-center gap-4">
                        <CheckCircle className="w-16 h-16 text-emerald-500" />
                        <h1 className="text-2xl font-bold">You're in!</h1>
                        <p className="text-gray-600 dark:text-gray-300">
                            Your subscription to the newsletter has been confirmed. Welcome aboard!
                        </p>
                        <Link 
                            to="/" 
                            className="mt-4 inline-flex items-center gap-2 bg-indigo-600 text-white px-6 py-2.5 rounded-lg font-semibold hover:bg-indigo-700 transition-all shadow-sm"
                        >
                            <ArrowLeft className="w-4 h-4" />
                            Back to Home
                        </Link>
                    </div>
                )}

                {status === 'error' && (
                    <div className="flex flex-col items-center gap-4">
                        <XCircle className="w-16 h-16 text-red-500" />
                        <h1 className="text-2xl font-bold">Oops!</h1>
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
