import React, { useState, useEffect } from 'react';
import { commentService } from '../services/commentService';
import { useAuth } from '../context/AuthContext';

interface CommentFormProps {
    articleId: string;
    parentCommentId?: string;
    onCommentCreated: () => void;
    onCancel?: () => void;
}

/**
 * A reusable form for creating new comments or replies.
 * Persists the author's name in localStorage for a better user experience.
 */
export const CommentForm: React.FC<CommentFormProps> = ({ 
    articleId, 
    parentCommentId, 
    onCommentCreated,
    onCancel 
}) => {
    const { user, isAuthenticated } = useAuth();
    const [content, setContent] = useState('');
    const [authorName, setAuthorName] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // HC: Initialize or update authorName based on auth status or local storage.
    useEffect(() => {
        if (isAuthenticated && user) {
            setAuthorName(user.username);
        } else {
            const savedName = localStorage.getItem('comment_author_name');
            if (savedName) {
                setAuthorName(savedName);
            }
        }
    }, [isAuthenticated, user]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        
        const finalAuthorName = isAuthenticated && user ? user.username : authorName.trim();

        if (!content.trim() || !finalAuthorName) {
            setError('Both name and comment content are required.');
            return;
        }

        setIsSubmitting(true);
        setError(null);

        try {
            await commentService.createComment({
                articleId,
                content: content.trim(),
                authorName: finalAuthorName,
                parentCommentId
            });

            // HC: Save the author name for future comments if not logged in.
            if (!isAuthenticated) {
                localStorage.setItem('comment_author_name', finalAuthorName);
            }
            
            setContent('');
            onCommentCreated();
        } catch (err: any) {
            console.error('Failed to post comment:', err);
            setError(err.response?.data?.message || 'Failed to post comment. Please try again.');
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="space-y-4 animate-in fade-in slide-in-from-top-2 duration-300">
            <div className="flex flex-col sm:flex-row sm:items-center gap-2">
                {isAuthenticated && user ? (
                    <div className="flex items-center gap-2 px-1">
                        <span className="text-xs font-semibold text-zinc-500 dark:text-zinc-400 uppercase tracking-wider">Commenting as:</span>
                        <span className="text-sm font-bold text-indigo-600 dark:text-indigo-400">{user.username}</span>
                    </div>
                ) : (
                    <input
                        type="text"
                        value={authorName}
                        onChange={(e) => setAuthorName(e.target.value)}
                        placeholder="Your Name"
                        required
                        disabled={isSubmitting}
                        className="w-full sm:w-64 px-4 py-2 bg-white dark:bg-zinc-800 border border-zinc-200 dark:border-zinc-700 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 text-zinc-900 dark:text-white text-sm transition-colors"
                    />
                )}
            </div>
            
            <textarea
                value={content}
                onChange={(e) => setContent(e.target.value)}
                placeholder={parentCommentId ? "Write a reply..." : "Write a comment..."}
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
                    disabled={isSubmitting || !content.trim() || (isAuthenticated ? false : !authorName.trim())}
                    className="px-6 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-bold rounded-xl shadow-sm disabled:opacity-50 transition-all focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 dark:focus:ring-offset-zinc-900"
                >
                    {isSubmitting ? 'Posting...' : (parentCommentId ? 'Post Reply' : 'Post Comment')}
                </button>
                
                {onCancel && (
                    <button
                        type="button"
                        onClick={onCancel}
                        disabled={isSubmitting}
                        className="px-4 py-2 text-zinc-500 dark:text-zinc-400 hover:text-zinc-800 dark:hover:text-zinc-200 text-sm font-medium transition-colors"
                    >
                        Cancel
                    </button>
                )}
            </div>
        </form>
    );
};

