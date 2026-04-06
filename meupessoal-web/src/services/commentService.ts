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
    createComment: async (request: CreateCommentRequest): Promise<{ id: string }> => {
        const response = await api.post<{ id: string }>('/comments', request);
        return response.data;
    }
};
