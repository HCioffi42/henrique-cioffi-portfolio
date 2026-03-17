namespace MeuSitePessoal.Application.Artigos.Queries.GetArtigos;

/// <summary>
/// HC: Represeting a summary of an article for list views.
/// </summary>
public class ArtigoSummaryDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Resumo { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public List<string> Tags { get; set; } = new();
}
