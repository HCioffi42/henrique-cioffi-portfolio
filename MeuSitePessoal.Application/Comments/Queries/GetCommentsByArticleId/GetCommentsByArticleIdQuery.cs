using MediatR;
using MeuSitePessoal.Application.Common.Models;

namespace MeuSitePessoal.Application.Comments.Queries.GetCommentsByArticleId;

/// <summary>
/// Query to retrieve a nested tree of comments for a specific article.
/// </summary>
/// <param name="ArticleId">The unique identifier of the article.</param>
public record GetCommentsByArticleIdQuery(Guid ArticleId) : IRequest<Result<List<CommentResponse>>>;

/// <summary>
/// DTO representing a comment in the hierarchical response.
/// </summary>
public class CommentResponse
{
    /// <summary>
    /// Gets or sets the comment ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the comment text.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author's name.
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the parent comment, if applicable.
    /// </summary>
    public Guid? ParentCommentId { get; set; }

    /// <summary>
    /// Gets or sets the collection of nested replies.
    /// </summary>
    public List<CommentResponse> Replies { get; set; } = new List<CommentResponse>();
}
