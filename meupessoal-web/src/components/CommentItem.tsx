import React, { useState } from 'react';
import { Reply, User, Calendar } from 'lucide-react';
import type { Comment } from '../models/Comment';
import { CommentForm } from './CommentForm';

interface CommentItemProps {
    comment: Comment;
    articleId: string;
    onCommentCreated: () => void;
    level?: number;
}

/**
 * Renders an individual comment and recursively renders its nested replies.
 * Visual nesting is achieved through progressive indentation and left borders.
 */
export const CommentItem: React.FC<CommentItemProps> = ({ 
    comment, 
    articleId, 
    onCommentCreated, 
    level = 0 
}) => {
    const [isReplying, setIsReplying] = useState(false);

    // HC: Formatting the date for a cleaner look.
    const formattedDate = new Date(comment.createdAt).toLocaleDateString(undefined, {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });

    return (
        <div className={`
            ${level > 0 ? 'ml-6 sm:ml-10 border-l border-zinc-100 dark:border-zinc-800 pl-6 sm:pl-8' : ''}
            mt-6 group animate-in fade-in slide-in-from-left-2 duration-300
        `}>
            <div className="flex flex-col gap-2">
                {/* HC: Comment Metadata Area */}
                <div className="flex items-center justify-between gap-3">
                    <div className="flex items-center gap-2">
                        <div className="w-8 h-8 rounded-full bg-zinc-100 dark:bg-zinc-800 flex items-center justify-center text-zinc-500 dark:text-zinc-400">
                            <User className="w-4 h-4" />
                        </div>
                        <span className="text-sm font-bold text-zinc-900 dark:text-white">
                            {comment.authorName}
                        </span>
                    </div>
                    <div className="flex items-center gap-1 text-[11px] text-zinc-500 dark:text-zinc-500 font-medium">
                        <Calendar className="w-3 h-3" />
                        {formattedDate}
                    </div>
                </div>

                {/* HC: Comment Content */}
                <div className="text-sm text-zinc-700 dark:text-zinc-300 leading-relaxed bg-zinc-50/50 dark:bg-zinc-900/50 p-4 rounded-2xl border border-zinc-100/50 dark:border-zinc-800/50">
                    {comment.content}
                </div>

                {/* HC: Reply Trigger */}
                <div className="flex items-center gap-4 mt-1">
                    <button
                        onClick={() => setIsReplying(!isReplying)}
                        className={`
                            flex items-center gap-1.5 text-xs font-bold transition-colors
                            ${isReplying 
                                ? 'text-indigo-600 dark:text-indigo-400' 
                                : 'text-zinc-500 hover:text-indigo-600 dark:text-zinc-500 dark:hover:text-indigo-400'}
                        `}
                    >
                        <Reply className="w-3 h-3" />
                        {isReplying ? 'Cancel Reply' : 'Reply'}
                    </button>
                </div>

                {/* HC: Nested Reply Form */}
                {isReplying && (
                    <div className="mt-4">
                        <CommentForm 
                            articleId={articleId} 
                            parentCommentId={comment.id}
                            onCommentCreated={() => {
                                setIsReplying(false);
                                onCommentCreated();
                            }}
                            onCancel={() => setIsReplying(false)}
                        />
                    </div>
                )}
            </div>

            {/* HC: Recursive rendering of nested replies. */}
            {comment.replies && comment.replies.length > 0 && (
                <div className="space-y-2">
                    {comment.replies.map(reply => (
                        <CommentItem 
                            key={reply.id} 
                            comment={reply} 
                            articleId={articleId} 
                            onCommentCreated={onCommentCreated}
                            level={level + 1}
                        />
                    ))}
                </div>
            )}
        </div>
    );
};
