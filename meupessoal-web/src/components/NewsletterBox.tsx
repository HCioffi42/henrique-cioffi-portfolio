import React, { useState } from 'react';
import { newsletterService } from '../services/newsletterService';

/**
 * Props for the NewsletterBox component.
 */
interface NewsletterBoxProps {
    /** The visual variant of the box. 'sidebar' is more compact and vertically stacked. */
    variant?: 'default' | 'sidebar';
}

/**
 * Renders a newsletter subscription box allowing users to opt-in with their email.
 * Supports a 'default' wide card variant and a 'sidebar' compact variant.
 */
export const NewsletterBox: React.FC<NewsletterBoxProps> = ({ variant = 'default' }) => {
    const [email, setEmail] = useState('');
    const [status, setStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle');
    const [message, setMessage] = useState('');

    const isSidebar = variant === 'sidebar';

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        
        if (!email) return;

        setStatus('loading');
        setMessage('');

        try {
            await newsletterService.subscribe({ email });
            setStatus('success');
            setMessage('Thank you for subscribing!');
            setEmail('');
        } catch (error: any) {
            setStatus('error');
            if (error.response?.status === 409) {
                setMessage(error.response.data?.message || 'This email is already subscribed.');
            } else if (error.response?.status === 400) {
                setMessage(error.response.data?.title || error.response.data?.message || 'Invalid email format.');
            } else {
                setMessage('An unexpected error occurred. Please try again.');
            }
        }
    };

    return (
        <div className={`
            ${isSidebar 
                ? 'bg-transparent p-0 border-none shadow-none w-full' 
                : 'bg-white dark:bg-zinc-900 rounded-2xl p-6 shadow-sm border border-zinc-200 dark:border-zinc-800 w-full max-w-md mx-auto'}
        `}>
            <h3 className={`
                ${isSidebar ? 'text-sm font-bold uppercase tracking-wider' : 'text-xl font-bold'} 
                text-zinc-900 dark:text-white mb-2
            `}>
                Newsletter
            </h3>
            <p className={`
                ${isSidebar ? 'text-[11px] leading-relaxed' : 'text-sm'} 
                text-zinc-500 dark:text-zinc-400 mb-4
            `}>
                Get the latest articles delivered directly to your inbox.
            </p>
            
            <form onSubmit={handleSubmit} className={`flex ${isSidebar ? 'flex-col' : 'flex-col sm:flex-row'} gap-2`}>
                <input
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="Email address"
                    required
                    disabled={status === 'loading'}
                    className={`
                        flex-1 px-3 py-1.5 bg-zinc-50 dark:bg-zinc-800 border border-zinc-200 dark:border-zinc-700 rounded-lg focus:outline-none focus:ring-1 focus:ring-indigo-500 text-zinc-900 dark:text-white disabled:opacity-50 transition-colors
                        ${isSidebar ? 'text-xs' : 'text-sm'}
                    `}
                />
                <button
                    type="submit"
                    disabled={status === 'loading'}
                    className={`
                        ${isSidebar ? 'w-full text-xs' : 'px-6 text-sm'} 
                        py-1.5 bg-indigo-600 hover:bg-indigo-700 text-white font-medium rounded-lg shadow-sm disabled:opacity-50 transition-colors focus:outline-none focus:ring-1 focus:ring-indigo-500
                    `}
                >
                    {status === 'loading' ? '...' : 'Subscribe'}
                </button>
            </form>
            
            {status === 'success' && (
                <p className="mt-4 text-sm text-emerald-600 dark:text-emerald-400 font-medium">{message}</p>
            )}
            
            {status === 'error' && (
                <p className="mt-4 text-sm text-red-600 dark:text-red-400 font-medium">{message}</p>
            )}
        </div>
    );
};
