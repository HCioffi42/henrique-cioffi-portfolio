import React, { useState, useEffect, useCallback } from 'react';
import { MessageSquare, AlertCircle } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import type { Comment } from '../models/Comment';
import { commentService } from '../services/commentService';
import { CommentItem } from './CommentItem';
import { CommentForm } from './CommentForm';

interface CommentSectionProps {
    articleId: string;
}

/**
 * Main container for the article comment system.
 * Handles fetching, reloading, and rendering the comment tree.
 */
export const CommentSection: React.FC<CommentSectionProps> = ({ articleId }) => {
    const { t } = useTranslation();
    const [comments, setComments] = useState<Comment[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // HC: Fetches or refreshes the comment list from the backend.
    const loadComments = useCallback(async () => {
        setIsLoading(true);
        setError(null);
        try {
            const data = await commentService.getCommentsByArticleId(articleId);
            setComments(data);
        } catch (err: unknown) {
            console.error('Failed to load comments:', err);
            setError(t('common.error'));
        } finally {
            setIsLoading(false);
        }
    }, [articleId, t]);

    useEffect(() => {
        void loadComments();
    }, [loadComments]);

    return (
        <section className="mt-16 pt-12 border-t border-zinc-100 dark:border-zinc-800">
            <div className="flex items-center justify-between mb-8">
                <h2 className="text-2xl font-bold text-zinc-900 dark:text-white flex items-center gap-3">
                    <MessageSquare className="w-6 h-6 text-indigo-600 dark:text-indigo-400" />
                    {t('article.discussion')}
                </h2>
                <div className="text-xs font-bold text-zinc-400 dark:text-zinc-500 uppercase tracking-widest bg-zinc-50 dark:bg-zinc-900 px-3 py-1 rounded-full border border-zinc-100 dark:border-zinc-800">
                    {comments.length} {t('article.thoughts')}
                </div>
            </div>

            {/* HC: Top-level Comment Form for new threads. */}
            <div className="mb-12 bg-zinc-50/50 dark:bg-zinc-900/50 p-6 rounded-3xl border border-zinc-100/50 dark:border-zinc-800/50">
                <h3 className="text-sm font-bold text-zinc-900 dark:text-white mb-4 uppercase tracking-tight">{t('article.leaveThought')}</h3>
                <CommentForm articleId={articleId} onCommentCreated={loadComments} />
            </div>

            {/* HC: Dynamic Rendering States */}
            {isLoading && comments.length === 0 ? (
                <div className="py-12 flex flex-col items-center justify-center gap-3 opacity-50">
                    <div className="animate-spin rounded-full h-8 w-8 border-2 border-indigo-600 border-t-transparent"></div>
                    <span className="text-sm font-medium text-zinc-500">{t('common.loading')}</span>
                </div>
            ) : error ? (
                <div className="py-12 flex flex-col items-center justify-center gap-3 text-red-500">
                    <AlertCircle className="w-8 h-8 opacity-20" />
                    <span className="text-sm font-medium">{error}</span>
                </div>
            ) : comments.length === 0 ? (
                <div className="py-16 flex flex-col items-center justify-center gap-4 text-center">
                    <div className="w-16 h-16 rounded-full bg-zinc-50 dark:bg-zinc-900 flex items-center justify-center text-zinc-200 dark:text-zinc-800 mb-2">
                        <MessageSquare className="w-8 h-8" />
                    </div>
                    <div className="space-y-1">
                        <p className="text-zinc-900 dark:text-white font-bold">{t('article.noComments')}</p>
                        <p className="text-zinc-500 dark:text-zinc-400 text-sm">{t('article.noCommentsDesc')}</p>
                    </div>
                </div>
            ) : (
                <div className="space-y-4">
                    {comments.map(comment => (
                        <CommentItem 
                            key={comment.id} 
                            comment={comment} 
                            articleId={articleId} 
                            onCommentCreated={loadComments} 
                        />
                    ))}
                </div>
            )}
        </section>
    );
};
