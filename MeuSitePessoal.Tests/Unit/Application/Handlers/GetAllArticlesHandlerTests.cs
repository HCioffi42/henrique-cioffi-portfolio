using Moq;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Application.Articles.Queries.GetAllArticles;
using MeuSitePessoal.Application.Common.Models;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Handlers;

public class GetAllArticlesHandlerTests
{
    private readonly Mock<IArticleRepository> _repositoryMock;
    private readonly GetAllArticlesHandler _handler;

    public GetAllArticlesHandlerTests()
    {
        _repositoryMock = new Mock<IArticleRepository>();
        _handler = new GetAllArticlesHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRetornarPagedListDeArtigosDoRepositorio()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var totalCount = 2;
        var artigos = new List<Article>
        {
            new Article("Title 1", "Content 1", "Summary 1", new List<string>(), ArticleCategory.Technology),
            new Article("Title 2", "Content 2", "Summary 2", new List<string>(), ArticleCategory.Technology)
        };
        
        // Mocking the new paginated method return
        _repositoryMock.Setup(r => r.GetPaginatedAsync(pageNumber, pageSize))
            .ReturnsAsync((artigos, totalCount));

        var query = new GetAllArticlesQuery(pageNumber, pageSize);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<PagedList<Article>>(result);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(artigos.Count, result.Items.Count);
        _repositoryMock.Verify(r => r.GetPaginatedAsync(pageNumber, pageSize), Times.Once);
    }
}