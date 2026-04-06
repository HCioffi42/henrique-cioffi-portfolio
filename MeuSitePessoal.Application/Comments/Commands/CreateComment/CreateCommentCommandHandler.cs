using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Application.Comments.Commands.CreateComment;

/// <summary>
/// Handles the creation of a new comment or reply.
/// Validates article existence and hierarchical consistency.
/// </summary>
public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Result<Guid>>
{
    private readonly BlogDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCommentCommandHandler"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public CreateCommentCommandHandler(BlogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Processes the comment creation request.
    /// </summary>
    /// <param name="request">The command details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the ID of the created comment.</returns>
    public async Task<Result<Guid>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify that the article exists.
        var articleExists = await _dbContext.Articles
            .AnyAsync(a => a.Id == request.ArticleId, cancellationToken);

        if (!articleExists)
        {
            return Result.Failure<Guid>("Article not found.", ErrorType.NotFound);
        }

        // 2. If it's a reply, verify that the parent comment exists and belongs to the same article.
        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _dbContext.Comments
                .FirstOrDefaultAsync(c => c.Id == request.ParentCommentId.Value, cancellationToken);

            if (parentComment == null)
            {
                return Result.Failure<Guid>("Parent comment not found.", ErrorType.NotFound);
            }

            if (parentComment.ArticleId != request.ArticleId)
            {
                return Result.Failure<Guid>("Parent comment does not belong to the specified article.", ErrorType.Conflict);
            }
        }

        // 3. Create and persist the comment.
        var comment = new Comment(
            request.ArticleId,
            request.Content,
            request.AuthorName,
            request.ParentCommentId
        );

        await _dbContext.Comments.AddAsync(comment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(comment.Id);
    }
}
