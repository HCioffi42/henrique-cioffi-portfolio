import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { newsletterService } from '../services/newsletterService';
import axios from 'axios';

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
    const { t } = useTranslation();
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
            setMessage(t('newsletter.success'));
            setEmail('');
        } catch (error: unknown) {
            setStatus('error');
            
            // HC: I implemented a generic error handling to ensure the user always sees feedback, even on 500 errors.
            if (axios.isAxiosError(error)) {
                const apiData = error.response?.data;
                const apiMessage = apiData?.message || apiData?.title;

                // HC: This block identifies specific business errors while providing a fallback for server failures.
                if (error.response?.status === 409) {
                    setMessage(apiMessage || t('newsletter.error'));
                } else {
                    setMessage(apiMessage || t('newsletter.error'));
                }
            } else {
                setMessage(t('common.error'));
            }
        }
    };

    return (
        <div className={`
            ${isSidebar 
                ? 'bg-transparent p-0 border-none shadow-none w-full' 
                : 'bg-white dark:bg-zinc-900 rounded-2xl p-8 shadow-sm border border-zinc-200 dark:border-zinc-800 w-full max-w-lg mx-auto'}
        `}>
            <h3 className={`
                ${isSidebar ? 'text-sm font-bold uppercase tracking-wider' : 'text-2xl font-bold'} 
                text-zinc-900 dark:text-white mb-2
            `}>
                {t('newsletter.title')}
            </h3>
            <p className={`
                ${isSidebar ? 'text-[11px] leading-relaxed' : 'text-base'} 
                text-zinc-500 dark:text-zinc-400 mb-6
            `}>
                {t('newsletter.subtitle')}
            </p>
            
            <form onSubmit={handleSubmit} className={`flex ${isSidebar ? 'flex-col' : 'flex-col sm:flex-row'} gap-3`}>
                <input
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder={t('newsletter.placeholder')}
                    required
                    disabled={status === 'loading'}
                    className={`
                        flex-1 px-4 py-2 bg-zinc-50 dark:bg-zinc-800 border border-zinc-200 dark:border-zinc-700 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 text-zinc-900 dark:text-white disabled:opacity-50 transition-all
                        ${isSidebar ? 'text-xs' : 'text-sm'}
                    `}
                />
                <button
                    type="submit"
                    disabled={status === 'loading'}
                    className={`
                        ${isSidebar ? 'w-full text-xs py-2' : 'px-8 text-sm py-2'} 
                        bg-indigo-600 hover:bg-indigo-700 text-white font-semibold rounded-xl shadow-md shadow-indigo-500/20 disabled:opacity-50 transition-all active:scale-[0.98] focus:outline-none focus:ring-2 focus:ring-indigo-500/20
                    `}
                >
                    {status === 'loading' ? '...' : t('newsletter.button')}
                </button>
            </form>
            
            {status === 'success' && (
                <p className="mt-4 text-xs text-emerald-600 dark:text-emerald-400 font-medium flex items-center gap-1.5 animate-in fade-in slide-in-from-top-1">
                    <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" /></svg>
                    {message}
                </p>
            )}
            
            {status === 'error' && (
                <p className="mt-4 text-xs text-red-600 dark:text-red-400 font-medium flex items-center gap-1.5 animate-in fade-in slide-in-from-top-1">
                    <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                    {message}
                </p>
            )}
        </div>
    );
};
