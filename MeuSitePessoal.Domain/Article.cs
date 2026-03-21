namespace MeuSitePessoal.Domain;

public class Article
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get;  set; }
    public string Summary { get; set; }
    public List<string> Tags { get; set; }
    public DateTime CreatedAt { get; set; }

    public Article(string title, string content, string summary, List<string> tags)
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
        CreatedAt = DateTime.UtcNow;
    }
}