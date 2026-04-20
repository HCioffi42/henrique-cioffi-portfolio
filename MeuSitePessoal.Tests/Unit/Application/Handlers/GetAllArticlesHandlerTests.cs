using Moq;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Application.Articles.Queries.GetAllArticles;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Application.Articles.Queries;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Handlers;

public class GetAllArticlesHandlerTests
{
    private readonly Mock<IArticleRepository> _repositoryMock;
    private readonly Mock<ILanguageProvider> _languageProviderMock;
    private readonly GetAllArticlesHandler _handler;

    public GetAllArticlesHandlerTests()
    {
        _repositoryMock = new Mock<IArticleRepository>();
        _languageProviderMock = new Mock<ILanguageProvider>();
        _languageProviderMock.Setup(x => x.GetCurrentLanguage()).Returns("en");
        
        _handler = new GetAllArticlesHandler(_repositoryMock.Object, _languageProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedListOfArticleResponse()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var totalCount = 2;
        var articles = new List<Article>
        {
            new Article("Title 1", "Título 1", "Content 1", "Conteúdo 1", "Summary 1", "Resumo 1", new List<string>(), ArticleCategory.Technology),
            new Article("Title 2", "Título 2", "Content 2", "Conteúdo 2", "Summary 2", "Resumo 2", new List<string>(), ArticleCategory.Technology)
        };
        
        _repositoryMock.Setup(r => r.GetPaginatedAsync(pageNumber, pageSize))
            .ReturnsAsync((articles, totalCount));

        var query = new GetAllArticlesQuery(pageNumber, pageSize);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<PagedList<ArticleResponse>>(result);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(articles.Count, result.Items.Count);
        Assert.Equal("Title 1", result.Items[0].Title);
        _repositoryMock.Verify(r => r.GetPaginatedAsync(pageNumber, pageSize), Times.Once);
    }
}