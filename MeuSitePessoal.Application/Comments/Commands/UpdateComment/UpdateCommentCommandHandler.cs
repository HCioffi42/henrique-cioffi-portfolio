using MediatR;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Application.Comments.Commands.UpdateComment;

/// <summary>
/// Handles the update of a comment's content.
/// Enforces ownership or admin role permissions.
/// </summary>
public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, Result<bool>>
{
    private readonly IBlogDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCommentCommandHandler(
        IBlogDbContext dbContext, 
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve the comment.
        var comment = await _dbContext.Comments
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (comment == null)
        {
            return Result.Failure<bool>("Comment not found.", ErrorType.NotFound);
        }

        // 2. Check permissions.
        var isOwner = _currentUserService.UserId == comment.UserId;
        var isAdmin = _currentUserService.IsInRole("Admin");

        if (!isOwner && !isAdmin)
        {
            return Result.Failure<bool>("Access denied: You can only update your own comments.", ErrorType.Failure);
        }

        // 3. Update the content.
        comment.Content = request.Content;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
