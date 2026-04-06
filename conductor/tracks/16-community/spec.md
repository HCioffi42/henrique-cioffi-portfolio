# Nested Comment System Specification (Task 16.2)

## 1. Overview
The objective is to implement a nested (threaded) comment system that allows users to post comments on articles and reply to existing comments. This system will support multiple levels of nesting.

## 2. API Contracts
### 2.1 `GET /api/articles/{articleId}/comments`
Returns a hierarchical tree of comments for a given article.

**Response Body (`CommentResponse`):**
```json
[
  {
    "id": "guid",
    "content": "string",
    "authorName": "string",
    "createdAt": "datetime",
    "parentCommentId": "guid?",
    "replies": [
      {
        "id": "guid",
        "content": "string",
        "authorName": "string",
        "createdAt": "datetime",
        "parentCommentId": "guid",
        "replies": []
      }
    ]
  }
]
```

### 2.2 `POST /api/comments`
Creates a new comment or reply.

**Request Body (`CreateCommentRequest`):**
```json
{
  "articleId": "guid",
  "content": "string",
  "authorName": "string",
  "parentCommentId": "guid?"
}
```

**Responses:**
*   **201 Created:** Returns the created comment.
*   **400 Bad Request:** Validation failed (empty content, invalid article/parent ID).
*   **404 Not Found:** Article or Parent comment not found.

## 3. Data Models
### 3.1 C# Domain Entity (`Comment`)
*   `Id` (Guid)
*   `Content` (string)
*   `AuthorName` (string)
*   `CreatedAt` (DateTime)
*   `ArticleId` (Guid)
*   `ParentCommentId` (Guid?)
*   **Navigation Properties:**
    *   `Article` (Article)
    *   `ParentComment` (Comment?)
    *   `Replies` (ICollection<Comment>)

### 3.2 C# CQRS Models
*   `CreateCommentCommand(Guid ArticleId, string Content, string AuthorName, Guid? ParentCommentId)`
*   `GetCommentsByArticleIdQuery(Guid ArticleId)`

### 3.3 TypeScript Interfaces
```typescript
export interface Comment {
    id: string;
    content: string;
    authorName: string;
    createdAt: string;
    parentCommentId?: string;
    replies: Comment[];
}

export interface CreateCommentRequest {
    articleId: string;
    content: string;
    authorName: string;
    parentCommentId?: string;
}
```

## 4. Architecture Design & Patterns
### 4.1 Backend (Clean Architecture)
*   **Domain:** Add `Comment` entity and update `Article` to have a `Comments` collection.
*   **Application:** Implement commands and handlers using MediatR.
    *   **Recursion Strategy (API):** The Query Handler will fetch all comments for an article and build a tree structure in memory before returning the response. This prevents multiple database round-trips for each nesting level.
*   **Infrastructure:** EF Core. Configure `Comment` entity in `BlogDbContext` with a self-referencing relationship for `ParentCommentId`.
*   **API:** Controller endpoints for getting and posting comments.

### 4.2 Frontend (meupessoal-web)
*   **Recursion Strategy (Rendering):**
    *   `CommentSection.tsx`: Fetches the comment tree and renders top-level `CommentItem` components.
    *   `CommentItem.tsx`: Renders the comment details and recursively renders its own `replies` using `CommentItem` again.
*   **Indentation:** Use CSS classes (e.g., `border-l-2 ml-4 pl-4`) to visually represent nesting levels.
*   **UI/UX:** Include a "Reply" button on each comment to toggle the `CommentForm` for that specific comment.
