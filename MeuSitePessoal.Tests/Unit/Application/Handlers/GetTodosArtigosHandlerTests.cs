using Moq;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Application.Artigos.Queries.GetTodosArtigos;
using MeuSitePessoal.Application.Common.Models;
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
    public async Task Handle_DeveRetornarPagedListDeArtigosDoRepositorio()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var totalCount = 2;
        var artigos = new List<Artigo>
        {
            new Artigo("Titulo 1", "Conteudo 1", "Resumo 1", new List<string>()),
            new Artigo("Titulo 2", "Conteudo 2", "Resumo 2", new List<string>())
        };
        
        // Mocking the new paginated method return
        _repositoryMock.Setup(r => r.ObterPaginadoAsync(pageNumber, pageSize))
            .ReturnsAsync((artigos, totalCount));

        var query = new GetTodosArtigosQuery(pageNumber, pageSize);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<PagedList<Artigo>>(result);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(artigos.Count, result.Items.Count);
        _repositoryMock.Verify(r => r.ObterPaginadoAsync(pageNumber, pageSize), Times.Once);
    }
}