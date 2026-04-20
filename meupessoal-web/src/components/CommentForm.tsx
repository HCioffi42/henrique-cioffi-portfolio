import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { commentService } from '../services/commentService';
import { useAuth } from '../context/AuthContext';
import axios from 'axios';
import { Link } from 'react-router-dom';

interface CommentFormProps {
    articleId: string;
    parentCommentId?: string;
    onCommentCreated: () => void;
    onCancel?: () => void;
}

/**
 * A reusable form for creating new comments or replies.
 * Requires authentication as of Track 16.4 (RBAC).
 */
export const CommentForm: React.FC<CommentFormProps> = ({ 
    articleId, 
    parentCommentId, 
    onCommentCreated,
    onCancel 
}) => {
    const { t } = useTranslation();
    const { user, isAuthenticated } = useAuth();
    const [content, setContent] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        
        if (!isAuthenticated || !user) {
            setError(t('article.loginRequired'));
            return;
        }

        if (!content.trim()) {
            setError(t('dashboard.required'));
            return;
        }

        setIsSubmitting(true);
        setError(null);

        try {
            await commentService.createComment({
                articleId,
                content: content.trim(),
                parentCommentId
            });
            
            setContent('');
            onCommentCreated();
        } catch (err: unknown) {
            console.error('Failed to post comment:', err);
            if (axios.isAxiosError(err)) {
                setError(err.response?.data?.message || t('common.error'));
            } else {
                setError(t('common.error'));
            }
        } finally {
            setIsSubmitting(false);
        }
    };

    if (!isAuthenticated) {
        return (
            <div className="p-6 bg-zinc-50 dark:bg-zinc-800/50 border border-dashed border-zinc-200 dark:border-zinc-700 rounded-2xl text-center">
                <p className="text-sm text-zinc-600 dark:text-zinc-400 mb-3">
                    {t('article.loginRequired')}
                </p>
                <Link 
                    to="/login" 
                    className="inline-flex items-center justify-center px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-bold rounded-xl transition-colors"
                >
                    {t('article.loginToComment')}
                </Link>
            </div>
        );
    }

    return (
        <form onSubmit={handleSubmit} className="space-y-4 animate-in fade-in slide-in-from-top-2 duration-300">
            <div className="flex items-center gap-2 px-1">
                <span className="text-xs font-semibold text-zinc-500 dark:text-zinc-400 uppercase tracking-wider">{t('article.commentingAs')}</span>
                <span className="text-sm font-bold text-indigo-600 dark:text-indigo-400">{user?.username}</span>
            </div>
            
            <textarea
                value={content}
                onChange={(e) => setContent(e.target.value)}
                placeholder={parentCommentId ? t('article.replyPlaceholder') : t('article.commentPlaceholder')}
                required
                disabled={isSubmitting}
                rows={3}
                className="w-full px-4 py-3 bg-white dark:bg-zinc-800 border border-zinc-200 dark:border-zinc-700 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 text-zinc-900 dark:text-white text-sm transition-colors resize-none"
            />

            {error && (
                <p className="text-xs text-red-600 dark:text-red-400 font-medium">{error}</p>
            )}

            <div className="flex items-center gap-3">
                <button
                    type="submit"
                    disabled={isSubmitting || !content.trim()}
                    className="px-6 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-bold rounded-xl shadow-sm disabled:opacity-50 transition-all focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 dark:focus:ring-offset-zinc-900"
                >
                    {isSubmitting ? t('article.posting') : (parentCommentId ? t('article.postReply') : t('article.postComment'))}
                </button>
                
                {onCancel && (
                    <button
                        type="button"
                        onClick={onCancel}
                        disabled={isSubmitting}
                        className="px-4 py-2 text-zinc-500 dark:text-zinc-400 hover:text-zinc-800 dark:hover:text-zinc-200 text-sm font-medium transition-colors"
                    >
                        {t('common.cancel')}
                    </button>
                )}
            </div>
        </form>
    );
};
