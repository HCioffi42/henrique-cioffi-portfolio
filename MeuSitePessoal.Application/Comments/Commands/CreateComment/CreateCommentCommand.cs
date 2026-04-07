using MediatR;
using MeuSitePessoal.Application.Common.Models;

namespace MeuSitePessoal.Application.Comments.Commands.CreateComment;

/// <summary>
/// Command to create a new comment or reply on an article.
/// </summary>
/// <param name="ArticleId">The unique identifier of the article.</param>
/// <param name="Content">The text content of the comment.</param>
/// <param name="ParentCommentId">The optional ID of the parent comment for nesting.</param>
public record CreateCommentCommand(
    Guid ArticleId,
    string Content,
    Guid? ParentCommentId = null
) : IRequest<Result<Guid>>;
