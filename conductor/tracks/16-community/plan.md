# Nested Comment System Implementation Plan (Task 16.2)

## Step 1: Backend Domain & Infrastructure
1.  **Create Entity:** `MeuSitePessoal.Domain/Entities/Comment.cs`
    *   Properties: `Id`, `Content`, `AuthorName`, `CreatedAt`, `ArticleId`, `ParentCommentId`.
    *   Navigation Properties: `Article`, `ParentComment`, `Replies`.
2.  **Update Article:** Add `ICollection<Comment> Comments` to `Article.cs`.
3.  **Update DbContext:** `MeuSitePessoal.Infrastructure/Data/BlogDbContext.cs`
    *   Add `public DbSet<Comment> Comments { get; set; }`.
    *   Configure relationship in `OnModelCreating`:
        *   Article has many Comments.
        *   Comment has optional ParentComment (self-reference).
4.  **Generate Migration:** `dotnet ef migrations add AddCommentEntity`.

## Step 2: Backend Application Layer (CQRS)
1.  **Create Command:** `CreateCommentCommand`
    *   Handler: Validates content, article ID, and parent comment ID. Saves to DB.
2.  **Create Query:** `GetCommentsByArticleIdQuery`
    *   **Recursion Strategy:**
        1. Fetch all comments for the article from the DB: `_dbContext.Comments.Where(c => c.ArticleId == articleId).ToListAsync()`.
        2. Create a dictionary to store `CommentResponse` objects indexed by their ID.
        3. Iterate through all comments:
            *   For each comment, create its `CommentResponse`.
            *   If it has a `ParentCommentId`, find the parent in the dictionary and add it to its `Replies` list.
            *   If not, add it to the root level list.
        4. Return the root level list.
3.  **Validation:** `CreateCommentCommandValidator` using FluentValidation.

## Step 3: Backend Presentation Layer (API)
1.  **Update Controller:** `MeuSitePessoal.Api/Controllers/ArticlesController.cs` or create `CommentsController.cs`.
    *   `GET /api/articles/{id}/comments`
    *   `POST /api/comments` (for new comments and replies).

## Step 4: Frontend Development (meupessoal-web)
1.  **Create Interfaces:** Define `Comment` and `CreateCommentRequest` in `models/Comment.ts`.
2.  **Create Service:** `services/commentService.ts` for API calls.
3.  **Create Components:**
    *   `src/components/CommentForm.tsx`: Standard text area and author name input.
    *   **Recursion Strategy (React):**
        *   `src/components/CommentItem.tsx`:
            ```tsx
            const CommentItem = ({ comment }) => (
              <div className="ml-4 border-l pl-4 my-4">
                <div className="font-bold">{comment.authorName}</div>
                <div>{comment.content}</div>
                <button>Reply</button>
                {comment.replies.map(reply => (
                  <CommentItem key={reply.id} comment={reply} />
                ))}
              </div>
            );
            ```
        *   `src/components/CommentSection.tsx`: Fetches comments and maps over the top-level list.
4.  **Integrate Component:** Place `<CommentSection articleId={id} />` in `ArticleDetails.tsx`.

## Step 5: Validation & Testing
1.  **Backend Tests:** Unit tests for Query and Command handlers.
2.  **Frontend Tests:** Ensure nested rendering works correctly and form submission updates the tree.
3.  **Integration:** Verify comments are saved and retrieved in hierarchical order.
