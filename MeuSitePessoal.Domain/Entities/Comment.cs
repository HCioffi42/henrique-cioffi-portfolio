using System.ComponentModel.DataAnnotations;

namespace MeuSitePessoal.Domain.Entities;

/// <summary>
/// Represents a comment or a reply posted by a user on a specific article.
/// Supports a hierarchical structure via the ParentCommentId reference.
/// </summary>
public class Comment
{
    /// <summary>
    /// Gets or sets the unique identifier for the comment.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the text content of the comment.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the individual who authored the comment.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp indicating when the comment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the article this comment belongs to.
    /// </summary>
    public Guid ArticleId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the parent article.
    /// </summary>
    public Article? Article { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the parent comment, if this is a reply.
    /// </summary>
    public Guid? ParentCommentId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the parent comment.
    /// </summary>
    public Comment? ParentComment { get; set; }

    /// <summary>
    /// Gets or sets the collection of replies nested under this comment.
    /// </summary>
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();

    /// <summary>
    /// Initializes a new instance of the <see cref="Comment"/> class.
    /// </summary>
    public Comment()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Comment"/> class with required fields.
    /// </summary>
    /// <param name="articleId">The ID of the associated article.</param>
    /// <param name="content">The text content of the comment.</param>
    /// <param name="authorName">The name of the author.</param>
    /// <param name="parentCommentId">The optional ID of the parent comment for nesting.</param>
    public Comment(Guid articleId, string content, string authorName, Guid? parentCommentId = null) : this()
    {
        ArticleId = articleId;
        Content = content;
        AuthorName = authorName;
        ParentCommentId = parentCommentId;
    }
}
