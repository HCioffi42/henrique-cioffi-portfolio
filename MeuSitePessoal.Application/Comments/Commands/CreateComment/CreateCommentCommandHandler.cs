using MediatR;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Application.Comments.Commands.CreateComment;

/// <summary>
/// Handles the creation of a new comment or reply.
/// Validates article existence, hierarchical consistency, and user authentication.
/// </summary>
public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Result<Guid>>
{
    private readonly IBlogDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCommentCommandHandler"/> class.
    /// </summary>
    public CreateCommentCommandHandler(
        IBlogDbContext dbContext, 
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    /// <summary>
    /// Processes the comment creation request.
    /// </summary>
    /// <param name="request">The command details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the ID of the created comment.</returns>
    public async Task<Result<Guid>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify user authentication.
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            return Result.Failure<Guid>("User must be authenticated to comment.", ErrorType.Failure);
        }

        // 2. Retrieve user info for the author name.
        var user = await _userManager.FindByIdAsync(_currentUserService.UserId);
        if (user == null)
        {
            return Result.Failure<Guid>("Authenticated user not found.", ErrorType.NotFound);
        }

        var authorName = user.UserName ?? user.Email ?? "Anonymous";

        // 3. Verify that the article exists.
        var articleExists = await _dbContext.Articles
            .AnyAsync(a => a.Id == request.ArticleId, cancellationToken);

        if (!articleExists)
        {
            return Result.Failure<Guid>("Article not found.", ErrorType.NotFound);
        }

        // 4. If it's a reply, verify that the parent comment exists and belongs to the same article.
        if (request.ParentCommentId.HasValue)
        {
            var parentCommentArticleId = await _dbContext.Comments
                .Where(c => c.Id == request.ParentCommentId.Value)
                .Select(c => (Guid?)c.ArticleId)
                .FirstOrDefaultAsync(cancellationToken);

            if (parentCommentArticleId == null)
            {
                return Result.Failure<Guid>("Parent comment not found.", ErrorType.NotFound);
            }

            if (parentCommentArticleId != request.ArticleId)
            {
                return Result.Failure<Guid>("Parent comment does not belong to the specified article.", ErrorType.Conflict);
            }
        }

        // 5. Create and persist the comment.
        var comment = new Comment(
            request.ArticleId,
            request.Content,
            authorName,
            _currentUserService.UserId,
            request.ParentCommentId
        );

        _dbContext.Comments.Add(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(comment.Id);
    }
}

