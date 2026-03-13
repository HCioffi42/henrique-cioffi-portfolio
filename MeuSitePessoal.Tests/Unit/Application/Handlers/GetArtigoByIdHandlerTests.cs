using Moq;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Application.Queries;
using MeuSitePessoal.Application.Handlers;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Handlers;

public class GetArtigoByIdHandlerTests
{
    private readonly Mock<IArtigoRepository> _repositoryMock;
    private readonly GetArtigoByIdHandler _handler;

    public GetArtigoByIdHandlerTests()
    {
        _repositoryMock = new Mock<IArtigoRepository>();
        _handler = new GetArtigoByIdHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_QuandoArtigoExiste_DeveRetornarArtigo()
    {
        // Arrange
        var artigo = new Artigo("Titulo", "Conteudo", "Resumo", new List<string>());
        var id = artigo.Id;
        _repositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(artigo);

        // Act
        var result = await _handler.Handle(new GetArtigoByIdQuery(id), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        _repositoryMock.Verify(r => r.ObterPorIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoArtigoNaoExiste_DeveRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Artigo?)null);

        // Act
        var result = await _handler.Handle(new GetArtigoByIdQuery(id), CancellationToken.None);

        // Assert
        Assert.Null(result);
        _repositoryMock.Verify(r => r.ObterPorIdAsync(id), Times.Once);
    }
}
