import api from './api';
import type { Comment, CreateCommentRequest } from '../models/Comment';

/**
 * Service to handle article comment operations.
 */
export const commentService = {
    /**
     * Retrieves a nested tree of comments for a specific article.
     * @param articleId - The unique identifier of the article.
     * @returns A promise that resolves to the hierarchical comment tree.
     */
    getCommentsByArticleId: async (articleId: string): Promise<Comment[]> => {
        const response = await api.get<Comment[]>(`/articles/${articleId}/comments`);
        return response.data;
    },

    /**
     * Sends a POST request to create a new comment or reply.
     * @param request - The data required to create the comment.
     * @returns A promise that resolves to the identifier of the newly created comment.
     */
    createComment: async (request: Omit<CreateCommentRequest, 'authorName'>): Promise<{ id: string }> => {
        // authorName is now removed from the request as the backend extracts it from the JWT.
        const response = await api.post<{ id: string }>('/comments', request);
        return response.data;
    },

    /**
     * Updates an existing comment's content.
     * @param id - The ID of the comment to update.
     * @param content - The new text content.
     */
    updateComment: async (id: string, content: string): Promise<void> => {
        await api.put(`/comments/${id}`, { id, content });
    },

    /**
     * Deletes a comment by its ID.
     * @param id - The ID of the comment to delete.
     */
    deleteComment: async (id: string): Promise<void> => {
        await api.delete(`/comments/${id}`);
    }
};
