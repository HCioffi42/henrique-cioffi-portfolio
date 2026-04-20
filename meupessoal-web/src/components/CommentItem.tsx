import React, { useState } from 'react';
import { Reply, User, Calendar, Edit2, Trash2, X, Check } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import type { Comment } from '../models/Comment';
import { CommentForm } from './CommentForm';
import { PermissionGate } from './PermissionGate';
import { commentService } from '../services/commentService';
import { toast } from 'react-hot-toast';

interface CommentItemProps {
    comment: Comment;
    articleId: string;
    onCommentCreated: () => void;
    level?: number;
}

/**
 * Renders an individual comment and recursively renders its nested replies.
 * Visual nesting is achieved through progressive indentation and left borders.
 * Supports Edit and Delete based on Track 16.4 RBAC.
 */
export const CommentItem: React.FC<CommentItemProps> = ({ 
    comment, 
    articleId, 
    onCommentCreated, 
    level = 0 
}) => {
    const { t } = useTranslation();
    const [isReplying, setIsReplying] = useState(false);
    const [isEditing, setIsEditing] = useState(false);
    const [editContent, setEditContent] = useState(comment.content);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const formattedDate = new Date(comment.createdAt).toLocaleDateString(undefined, {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });

    const handleUpdate = async () => {
        if (!editContent.trim() || editContent === comment.content) {
            setIsEditing(false);
            return;
        }

        setIsSubmitting(true);
        try {
            await commentService.updateComment(comment.id, editContent.trim());
            toast.success(t('article.commentUpdated'));
            setIsEditing(false);
            onCommentCreated(); // Refresh comments
        } catch (error) {
            console.error('Failed to update comment:', error);
            toast.error(t('article.commentUpdateFailed'));
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDelete = async () => {
        if (!window.confirm(t('common.confirmDelete'))) return;

        try {
            await commentService.deleteComment(comment.id);
            toast.success(t('article.commentDeleted'));
            onCommentCreated(); // Refresh comments
        } catch (error) {
            console.error('Failed to delete comment:', error);
            toast.error(t('article.commentDeleteFailed'));
        }
    };

    return (
        <div className={`
            ${level > 0 ? 'ml-6 sm:ml-10 border-l border-zinc-100 dark:border-zinc-800 pl-6 sm:pl-8' : ''}
            mt-6 group animate-in fade-in slide-in-from-left-2 duration-300
        `}>
            <div className="flex flex-col gap-2">
                <div className="flex items-center justify-between gap-3">
                    <div className="flex items-center gap-2">
                        <div className="w-8 h-8 rounded-full bg-zinc-100 dark:bg-zinc-800 flex items-center justify-center text-zinc-500 dark:text-zinc-400">
                            <User className="w-4 h-4" />
                        </div>
                        <span className="text-sm font-bold text-zinc-900 dark:text-white">
                            {comment.authorName}
                        </span>
                    </div>
                    <div className="flex items-center gap-3">
                        <div className="flex items-center gap-1 text-[11px] text-zinc-500 dark:text-zinc-500 font-medium">
                            <Calendar className="w-3 h-3" />
                            {formattedDate}
                        </div>
                        
                        <PermissionGate ownerId={comment.userId}>
                            <div className="flex items-center gap-2">
                                <button 
                                    onClick={() => setIsEditing(true)}
                                    className="p-1 text-zinc-400 hover:text-indigo-600 transition-colors"
                                    title={t('article.edit')}
                                >
                                    <Edit2 className="w-3 h-3" />
                                </button>
                                <button 
                                    onClick={handleDelete}
                                    className="p-1 text-zinc-400 hover:text-red-600 transition-colors"
                                    title={t('common.delete')}
                                >
                                    <Trash2 className="w-3 h-3" />
                                </button>
                            </div>
                        </PermissionGate>
                    </div>
                </div>

                {isEditing ? (
                    <div className="space-y-3 mt-1">
                        <textarea
                            value={editContent}
                            onChange={(e) => setEditContent(e.target.value)}
                            className="w-full px-4 py-3 bg-white dark:bg-zinc-800 border border-zinc-200 dark:border-zinc-700 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 text-zinc-900 dark:text-white text-sm transition-colors resize-none"
                            rows={3}
                            autoFocus
                        />
                        <div className="flex items-center gap-2">
                            <button
                                onClick={handleUpdate}
                                disabled={isSubmitting || !editContent.trim()}
                                className="flex items-center gap-1.5 px-3 py-1.5 bg-indigo-600 text-white text-xs font-bold rounded-lg hover:bg-indigo-700 transition-colors"
                            >
                                <Check className="w-3 h-3" />
                                {t('common.save')}
                            </button>
                            <button
                                onClick={() => {
                                    setIsEditing(false);
                                    setEditContent(comment.content);
                                }}
                                disabled={isSubmitting}
                                className="flex items-center gap-1.5 px-3 py-1.5 bg-zinc-100 dark:bg-zinc-800 text-zinc-600 dark:text-zinc-400 text-xs font-bold rounded-lg hover:bg-zinc-200 dark:hover:bg-zinc-700 transition-colors"
                            >
                                <X className="w-3 h-3" />
                                {t('common.cancel')}
                            </button>
                        </div>
                    </div>
                ) : (
                    <div className="text-sm text-zinc-700 dark:text-zinc-300 leading-relaxed bg-zinc-50/50 dark:bg-zinc-900/50 p-4 rounded-2xl border border-zinc-100/50 dark:border-zinc-800/50">
                        {comment.content}
                    </div>
                )}

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
                        {isReplying ? t('article.cancelReply') : t('article.reply')}
                    </button>
                </div>

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
