using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeuSitePessoal.Application.Articles.Queries.GetArticles;
using MeuSitePessoal.Application.Articles.Queries.GetArticlesSearch;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Application.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Articles.Queries;

/// <summary>
/// Unit tests for the GetArticlesSearchQueryHandler using an In-Memory database.
/// </summary>
public class GetArticlesSearchQueryHandlerTests
{
    private readonly IMemoryCache _cache;
    private readonly Mock<IArticleSearchService> _searchServiceMock;
    private readonly Mock<ILanguageProvider> _languageProviderMock;

    public GetArticlesSearchQueryHandlerTests()
    {
        // HC: Initializes a real MemoryCache instance to be used across unit tests.
        _cache = new MemoryCache(new MemoryCacheOptions());
        _searchServiceMock = new Mock<IArticleSearchService>();
        _languageProviderMock = new Mock<ILanguageProvider>();
        _languageProviderMock.Setup(x => x.GetCurrentLanguage()).Returns("en");
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldReturnResultsFromService()
    {
        // Arrange
        var searchTerm = "Keyword";
        var expectedItems = new List<ArticleSummaryDto>
        {
            new ArticleSummaryDto { Id = Guid.NewGuid(), Title = "Keyword Title" }
        };
        var expectedResult = new PagedResult<ArticleSummaryDto>(expectedItems, 1, 1, 1);

        _searchServiceMock
            .Setup(s => s.SearchAsync(searchTerm, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var handler = new GetArticlesSearchQueryHandler(_searchServiceMock.Object, _cache, _languageProviderMock.Object);
        var query = new GetArticlesSearchQuery(SearchTerm: searchTerm);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("Keyword Title", result.Items.First().Title);
        _searchServiceMock.Verify(s => s.SearchAsync(searchTerm, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithEmptySearchTerm_ShouldCallServiceCorrectly()
    {
        // Arrange
        var expectedItems = new List<ArticleSummaryDto>
        {
            new ArticleSummaryDto { Id = Guid.NewGuid(), Title = "A" },
            new ArticleSummaryDto { Id = Guid.NewGuid(), Title = "B" }
        };
        var expectedResult = new PagedResult<ArticleSummaryDto>(expectedItems, 2, 1, 10);

        _searchServiceMock
            .Setup(s => s.SearchAsync("", It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var handler = new GetArticlesSearchQueryHandler(_searchServiceMock.Object, _cache, _languageProviderMock.Object);
        var query = new GetArticlesSearchQuery(SearchTerm: "");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Items.Count);
        _searchServiceMock.Verify(s => s.SearchAsync("", 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithPagination_ShouldPassParametersToService()
    {
        // Arrange
        var expectedItems = new List<ArticleSummaryDto>
        {
            new ArticleSummaryDto { Title = "Match 1" },
            new ArticleSummaryDto { Title = "Match 2" }
        };
        var expectedResult = new PagedResult<ArticleSummaryDto>(expectedItems, 5, 2, 2);

        _searchServiceMock
            .Setup(s => s.SearchAsync("Match", 2, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var handler = new GetArticlesSearchQueryHandler(_searchServiceMock.Object, _cache, _languageProviderMock.Object);
        var query = new GetArticlesSearchQuery(SearchTerm: "Match", PageNumber: 2, PageSize: 2);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        _searchServiceMock.Verify(s => s.SearchAsync("Match", 2, 2, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenCalledTwice_ShouldReturnFromCache()
    {
        // Arrange
        var searchTerm = "CacheTest";
        var expectedResult = new PagedResult<ArticleSummaryDto>(new List<ArticleSummaryDto>(), 0, 1, 10);

        _searchServiceMock
            .Setup(s => s.SearchAsync(searchTerm, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var handler = new GetArticlesSearchQueryHandler(_searchServiceMock.Object, _cache, _languageProviderMock.Object);
        var query = new GetArticlesSearchQuery(SearchTerm: searchTerm);

        // Act
        await handler.Handle(query, CancellationToken.None); // First call hits service
        await handler.Handle(query, CancellationToken.None); // Second call should hit cache

        // Assert
        _searchServiceMock.Verify(s => s.SearchAsync(searchTerm, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var expectedResult = new PagedResult<ArticleSummaryDto>(new List<ArticleSummaryDto>(), 0, 1, 10);

        _searchServiceMock
            .Setup(s => s.SearchAsync("XYZ", It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var handler = new GetArticlesSearchQueryHandler(_searchServiceMock.Object, _cache, _languageProviderMock.Object);
        var query = new GetArticlesSearchQuery(SearchTerm: "XYZ");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}