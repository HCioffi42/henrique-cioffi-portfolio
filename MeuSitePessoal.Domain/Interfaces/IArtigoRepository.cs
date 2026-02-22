using MeuSitePessoal.Domain;

namespace MeuSitePessoal.Domain.Interfaces;

public interface IArtigoRepository
{
    // Define o contrato para buscar todos os artigos de forma assíncrona.
    Task<IEnumerable<Artigo>> ObterTodosAsync();
    
    // Define o contrato para salvar um novo artigo no banco de dados.
    Task AdicionarAsync(Artigo artigo);
}