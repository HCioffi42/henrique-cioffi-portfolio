namespace MeuSitePessoal.Domain;

public class Artigo
{
    public Guid Id { get; set; }
    public string Titulo { get; set; }
    public string Conteudo { get;  set; }
    public string Resumo { get; set; }
    public List<string> Tags { get; set; }
    public DateTime DataCriacao { get; set; }

    public Artigo(string titulo, string conteudo, string resumo, List<string> tags)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new ArgumentException("O título do artigo é obrigatório.");
        
        if (string.IsNullOrWhiteSpace(conteudo))
            throw new ArgumentException("O conteúdo do artigo é obrigatório.");

        Id = Guid.NewGuid();
        Titulo = titulo;
        Conteudo = conteudo;
        Resumo = resumo;
        Tags = tags ?? new List<string>();
        DataCriacao = DateTime.UtcNow;
    }
}