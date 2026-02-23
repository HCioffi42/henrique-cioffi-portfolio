namespace MeuSitePessoal.Domain;

public class Artigo
{
    public Guid Id { get; set; }
    public string Titulo { get; set; }
    public string Conteudo { get;  set; }
    public DateTime DataCriacao { get; set; }

    public Artigo(string titulo, string conteudo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new ArgumentException("O título do artigo é obrigatório.");

        Id = Guid.NewGuid();
        Titulo = titulo;
        Conteudo = conteudo;
        DataCriacao = DateTime.UtcNow;
    }
}
// O código define a entidade Artigo com validação básica no construtor para garantir a integridade dos dados iniciais.