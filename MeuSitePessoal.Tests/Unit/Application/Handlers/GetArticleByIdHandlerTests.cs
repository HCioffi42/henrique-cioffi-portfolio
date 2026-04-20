using MeuSitePessoal.Application.Articles.Queries.GetArticleById;
using Moq;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Domain.Entities;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Handlers;

public class GetArticleByIdHandlerTests
{
    private readonly Mock<IArticleRepository> _repositoryMock;
    private readonly GetArticleByIdHandler _handler;

    public GetArticleByIdHandlerTests()
    {
        _repositoryMock = new Mock<IArticleRepository>();
        var languageProviderMock = new Mock<ILanguageProvider>();
        languageProviderMock.Setup(x => x.GetCurrentLanguage()).Returns("en");
        _handler = new GetArticleByIdHandler(_repositoryMock.Object, languageProviderMock.Object);
    }

    [Fact]
    public async Task Handle_QuandoArtigoExiste_DeveRetornarArtigo()
    {
        // Arrange
        var artigo = new Article("Title", "Title", "Content", "Content", "Summary", "Summary", new List<string>(), ArticleCategory.Technology);
        var id = artigo.Id;
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(artigo);

        // Act
        var result = await _handler.Handle(new GetArticleByIdQuery(id), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        _repositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoArtigoNaoExiste_DeveRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Article?)null);

        // Act
        var result = await _handler.Handle(new GetArticleByIdQuery(id), CancellationToken.None);

        // Assert
        Assert.Null(result);
        _repositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
    }
}
