using MediatR;
using MeuSitePessoal.Application.Common.Models;

namespace MeuSitePessoal.Application.Comments.Commands.UpdateComment;

/// <summary>
/// Command to update the content of an existing comment.
/// </summary>
/// <param name="Id">The unique identifier of the comment.</param>
/// <param name="Content">The new text content.</param>
public record UpdateCommentCommand(Guid Id, string Content) : IRequest<Result<bool>>;
