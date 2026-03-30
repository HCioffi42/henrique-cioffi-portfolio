using MediatR;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;

namespace MeuSitePessoal.Application.Articles.Commands.CreateArticle;

public class CreateArticleHandler : IRequestHandler<CreateArticleCommand, Guid>
{
    private readonly IArticleRepository _repository;

    // Initializes the handler with the necessary repository through dependency injection.
    public CreateArticleHandler(IArticleRepository repository)
    {
        _repository = repository;
    }

    // Processes the article creation request by mapping command data to a new domain entity.
    public async Task<Guid> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
    {
        // Creates a new Article instance with the provided Title, Content, Summary, and Tags.
        var artigo = new Article(
            request.Title, 
            request.Content, 
            request.Summary, 
            request.Tags.Select(t => t.ToLower()).ToList(), // Normalizes tags to lowercase for consistent storage and querying.
            request.Category
        );
        
        // Persists the article entity into the PostgreSQL database via the infrastructure layer.
        await _repository.AddAsync(artigo);
        
        // Returns the unique identifier assigned to the newly created article.
        return artigo.Id;
    }
}
