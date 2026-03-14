using MediatR;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;

namespace MeuSitePessoal.Application.Artigos.Commands.CreateArtigo;

public class CreateArtigoHandler : IRequestHandler<CreateArtigoCommand, Guid>
{
    private readonly IArtigoRepository _repository;

    // Initializes the handler with the necessary repository through dependency injection.
    public CreateArtigoHandler(IArtigoRepository repository)
    {
        _repository = repository;
    }

    // Processes the article creation request by mapping command data to a new domain entity.
    public async Task<Guid> Handle(CreateArtigoCommand request, CancellationToken cancellationToken)
    {
        // Creates a new Artigo instance with the provided Title, Content, Summary, and Tags.
        var artigo = new Artigo(
            request.Titulo, 
            request.Conteudo, 
            request.Resumo, 
            request.Tags
        );
        
        // Persists the article entity into the PostgreSQL database via the infrastructure layer.
        await _repository.AdicionarAsync(artigo);
        
        // Returns the unique identifier assigned to the newly created article.
        return artigo.Id;
    }
}
