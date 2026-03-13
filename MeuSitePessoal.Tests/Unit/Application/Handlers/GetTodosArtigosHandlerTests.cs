using Moq;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Application.Artigos.Queries.GetTodosArtigos;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Handlers;

public class GetTodosArtigosHandlerTests
{
    private readonly Mock<IArtigoRepository> _repositoryMock;
    private readonly GetTodosArtigosHandler _handler;

    public GetTodosArtigosHandlerTests()
    {
        _repositoryMock = new Mock<IArtigoRepository>();
        _handler = new GetTodosArtigosHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRetornarListaDeArtigosDoRepositorio()
    {
        // Arrange
        var artigos = new List<Artigo>
        {
            new Artigo("Titulo 1", "Conteudo 1", "Resumo 1", new List<string>()),
            new Artigo("Titulo 2", "Conteudo 2", "Resumo 2", new List<string>())
        };
        _repositoryMock.Setup(r => r.ObterTodosAsync()).ReturnsAsync(artigos);

        // Act
        var result = await _handler.Handle(new GetTodosArtigosQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(artigos.Count, result.Count());
        _repositoryMock.Verify(r => r.ObterTodosAsync(), Times.Once);
    }
}
