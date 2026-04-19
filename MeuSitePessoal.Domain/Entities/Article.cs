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

    private string _title = string.Empty;
    private string _content = string.Empty;
    private string _summary = string.Empty;

    // Legacy fields (maintained for migration, will be removed or ignored after transition)
    public string Title 
    { 
        get => _title; 
        set 
        { 
            _title = value; 
            TitleEn = value; 
            TitlePt = value; 
        } 
    }
    public string Content 
    { 
        get => _content; 
        set 
        { 
            _content = value; 
            ContentEn = value; 
            ContentPt = value; 
        } 
    }
    public string Summary 
    { 
        get => _summary; 
        set 
        { 
            _summary = value; 
            SummaryEn = value; 
            SummaryPt = value; 
        } 
    }

    public List<string> Tags { get; set; }
    public ArticleCategory Category { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the collection of comments posted by users on this article.
    /// </summary>
    public ICollection<Entities.Comment> Comments { get; set; } = new List<Entities.Comment>();

    // Parameterless constructor for EF Core
    private Article() { Tags = new List<string>(); }

    public Article(string title, string content, string summary, List<string> tags, ArticleCategory category)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Summary cannot be empty.", nameof(summary));

        Id = Guid.NewGuid();
        
        // HC: Keep all 9 columns in sync during the transition.
        Title = title;
        Content = content;
        Summary = summary;

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
    /// Updates the article content for a specific language and keeps legacy fields in sync.
    /// </summary>
    public void UpdateContent(string language, string title, string content, string summary)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Summary cannot be empty.", nameof(summary));

        // Update legacy fields for "Split Brain" synchronization
        Title = title;
        Content = content;
        Summary = summary;

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
