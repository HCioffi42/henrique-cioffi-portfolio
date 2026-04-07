using MediatR;
using MeuSitePessoal.Application.Common.Models;

namespace MeuSitePessoal.Application.Comments.Commands.DeleteComment;

/// <summary>
/// Command to delete a comment.
/// </summary>
/// <param name="Id">The unique identifier of the comment to delete.</param>
public record DeleteCommentCommand(Guid Id) : IRequest<Result<bool>>;
