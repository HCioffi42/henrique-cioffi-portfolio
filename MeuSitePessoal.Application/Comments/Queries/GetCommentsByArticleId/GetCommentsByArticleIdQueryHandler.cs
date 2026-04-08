using MediatR;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Application.Comments.Queries.GetCommentsByArticleId;

/// <summary>
/// Handles the retrieval and hierarchical assembly of comments for a specific article.
/// Implements an O(n) in-memory tree construction strategy.
/// </summary>
public class GetCommentsByArticleIdQueryHandler : IRequestHandler<GetCommentsByArticleIdQuery, Result<List<CommentResponse>>>
{
    private readonly IBlogDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCommentsByArticleIdQueryHandler"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public GetCommentsByArticleIdQueryHandler(IBlogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Fetches all comments for the article and assembles them into a nested tree.
    /// </summary>
    /// <param name="request">The query details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the hierarchical list of comments.</returns>
    public async Task<Result<List<CommentResponse>>> Handle(GetCommentsByArticleIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Verify that the article exists to avoid ambiguous empty results.
        var articleExists = await _dbContext.Articles
            .AsNoTracking()
            .AnyAsync(a => a.Id == request.ArticleId, cancellationToken);

        if (!articleExists)
        {
            return Result.Failure<List<CommentResponse>>("Article not found.", ErrorType.NotFound);
        }

        // 2. Fetch all comments associated with the article in a single query.
        // We order by CreatedAt to ensure a predictable sequence.
        var allComments = await _dbContext.Comments
            .AsNoTracking()
            .Where(c => c.ArticleId == request.ArticleId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        // 3. Assemble the tree in memory using a dictionary for fast lookup.
        var commentMap = new Dictionary<Guid, CommentResponse>();
        var rootComments = new List<CommentResponse>();

        // Phase 1: Create all response DTOs and populate the lookup map.
        foreach (var comment in allComments)
        {
            var response = new CommentResponse
            {
                Id = comment.Id,
                Content = comment.Content,
                AuthorName = comment.AuthorName,
                CreatedAt = comment.CreatedAt,
                ParentCommentId = comment.ParentCommentId,
                Replies = new List<CommentResponse>()
            };
            commentMap[response.Id] = response;
        }

        // Phase 2: Link replies to their parents or identify them as root comments.
        foreach (var comment in allComments)
        {
            var response = commentMap[comment.Id];

            if (comment.ParentCommentId.HasValue && commentMap.TryGetValue(comment.ParentCommentId.Value, out var parent))
            {
                // This is a reply; nest it under the resolved parent.
                parent.Replies.Add(response);
            }
            else
            {
                // This is either a top-level comment or its parent was not found (orphan protection).
                rootComments.Add(response);
            }
        }

        return Result.Success(rootComments);
    }
}
