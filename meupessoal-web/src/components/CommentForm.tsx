import React, { useState, useEffect } from 'react';
import { commentService } from '../services/commentService';

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
    const [content, setContent] = useState('');
    const [authorName, setAuthorName] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // HC: Load the last used author name from local storage.
    useEffect(() => {
        const savedName = localStorage.getItem('comment_author_name');
        if (savedName) {
            setAuthorName(savedName);
        }
    }, []);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        
        if (!content.trim() || !authorName.trim()) {
            setError('Both name and comment content are required.');
            return;
        }

        setIsSubmitting(true);
        setError(null);

        try {
            await commentService.createComment({
                articleId,
                content: content.trim(),
                authorName: authorName.trim(),
                parentCommentId
            });

            // HC: Save the author name for future comments.
            localStorage.setItem('comment_author_name', authorName.trim());
            
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
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <input
                    type="text"
                    value={authorName}
                    onChange={(e) => setAuthorName(e.target.value)}
                    placeholder="Your Name"
                    required
                    disabled={isSubmitting}
                    className="px-4 py-2 bg-white dark:bg-zinc-800 border border-zinc-200 dark:border-zinc-700 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 text-zinc-900 dark:text-white text-sm transition-colors"
                />
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
                    disabled={isSubmitting || !content.trim() || !authorName.trim()}
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
