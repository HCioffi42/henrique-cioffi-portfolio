using MediatR;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Application.Comments.Commands.DeleteComment;

/// <summary>
/// Handles the deletion of a comment.
/// Enforces ownership or admin role permissions.
/// </summary>
public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Result<bool>>
{
    private readonly BlogDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCommentCommandHandler(
        BlogDbContext dbContext, 
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
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
            return Result.Failure<bool>("Access denied: You can only delete your own comments.", ErrorType.Failure);
        }

        // 3. Remove the comment.
        _dbContext.Comments.Remove(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
