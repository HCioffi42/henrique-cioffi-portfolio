/**
 * Represents a single comment or reply in the hierarchical comment system.
 */
export interface Comment {
    /** The unique identifier for the comment. */
    id: string;
    /** The text content of the comment. */
    content: string;
    /** The name of the author who wrote the comment. */
    authorName: string;
    /** The ID of the author who wrote the comment. */
    userId?: string;
    /** The timestamp of when the comment was posted. */
    createdAt: string;
    /** The optional identifier for the parent comment if this is a reply. */
    parentCommentId?: string;
    /** The nested collection of replies for this comment. */
    replies: Comment[];
}

/**
 * Data required to create a new comment or reply.
 */
export interface CreateCommentRequest {
    /** The unique identifier of the article. */
    articleId: string;
    /** The text content of the comment. */
    content: string;
    /** The name of the author. */
    authorName: string;
    /** The optional identifier for the parent comment for nesting. */
    parentCommentId?: string;
}
