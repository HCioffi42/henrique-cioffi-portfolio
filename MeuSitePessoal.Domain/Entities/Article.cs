namespace MeuSitePessoal.Domain.Entities;

/// <summary>
/// Represents a blog article with support for multiple languages using a Flat Schema approach.
/// </summary>
public class Article
{
    public Guid Id { get; set; }

    // English Content
    public string TitleEn { get; set; } = string.Empty;
    public string ContentEn { get; set; } = string.Empty;
    public string SummaryEn { get; set; } = string.Empty;

    // Portuguese Content
    public string TitlePt { get; set; } = string.Empty;
    public string ContentPt { get; set; } = string.Empty;
    public string SummaryPt { get; set; } = string.Empty;

    // Legacy fields (maintained for backward compatibility with existing data)
    // HC: SET is now independent to prevent mirroring logic. 
    // New registers should ideally have these as null or empty, but we keep the properties for EF compatibility.
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;

    public List<string> Tags { get; set; }
    public ArticleCategory Category { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the collection of comments posted by users on this article.
    /// </summary>
    public ICollection<Entities.Comment> Comments { get; set; } = new List<Entities.Comment>();

    // Parameterless constructor for EF Core
    private Article() { Tags = new List<string>(); }

    public Article(
        string titleEn, string titlePt,
        string contentEn, string contentPt,
        string summaryEn, string summaryPt,
        List<string> tags, ArticleCategory category)
    {
        if (string.IsNullOrWhiteSpace(titleEn)) throw new ArgumentException("English Title cannot be empty.", nameof(titleEn));
        if (string.IsNullOrWhiteSpace(titlePt)) throw new ArgumentException("Portuguese Title cannot be empty.", nameof(titlePt));
        if (string.IsNullOrWhiteSpace(contentEn)) throw new ArgumentException("English Content cannot be empty.", nameof(contentEn));
        if (string.IsNullOrWhiteSpace(contentPt)) throw new ArgumentException("Portuguese Content cannot be empty.", nameof(contentPt));
        if (string.IsNullOrWhiteSpace(summaryEn)) throw new ArgumentException("English Summary cannot be empty.", nameof(summaryEn));
        if (string.IsNullOrWhiteSpace(summaryPt)) throw new ArgumentException("Portuguese Summary cannot be empty.", nameof(summaryPt));

        Id = Guid.NewGuid();
        
        // Localized fields
        TitleEn = titleEn;
        TitlePt = titlePt;
        ContentEn = contentEn;
        ContentPt = contentPt;
        SummaryEn = summaryEn;
        SummaryPt = summaryPt;

        // HC: We no longer populate legacy fields for new records.
        // They will remain empty/null in the DB to avoid "Split Brain" issues.
        Title = string.Empty;
        Content = string.Empty;
        Summary = string.Empty;

        Tags = tags ?? new List<string>();
        Category = category;
        CreatedAt = DateTime.UtcNow;
    }

    [Obsolete("Use the multi-language constructor instead.")]
    public Article(string title, string content, string summary, List<string> tags, ArticleCategory category)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Summary cannot be empty.", nameof(summary));

        Id = Guid.NewGuid();
        
        // Populating legacy fields for old-style creation (migration only)
        Title = title;
        Content = content;
        Summary = summary;

        // Also filling EN/PT for safety during transition
        TitleEn = title;
        ContentEn = content;
        SummaryEn = summary;
        TitlePt = title;
        ContentPt = content;
        SummaryPt = summary;

        Tags = tags ?? new List<string>();
        Category = category;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the article content for a specific language.
    /// HC: No longer updates legacy fields.
    /// </summary>
    public void UpdateContent(string language, string title, string content, string summary)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Summary cannot be empty.", nameof(summary));

        if (language.ToLower().StartsWith("pt"))
        {
            TitlePt = title;
            ContentPt = content;
            SummaryPt = summary;
        }
        else
        {
            TitleEn = title;
            ContentEn = content;
            SummaryEn = summary;
        }
    }
}
