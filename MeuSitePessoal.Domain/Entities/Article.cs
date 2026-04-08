namespace MeuSitePessoal.Domain.Entities;

public class Article
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get;  set; }
    public string Summary { get; set; }
    public List<string> Tags { get; set; }
    public ArticleCategory Category { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the collection of comments posted by users on this article.
    /// </summary>
    public ICollection<Entities.Comment> Comments { get; set; } = new List<Entities.Comment>();

    public Article(string title, string content, string summary, List<string> tags, ArticleCategory category)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("The title of the article is required.");
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("The content of the article is required.");

        Id = Guid.NewGuid();
        Title = title;
        Content = content;
        Summary = summary;
        Tags = tags ?? new List<string>();
        Category = category;
        CreatedAt = DateTime.UtcNow;
    }
}