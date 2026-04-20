using System.Net;
using System.Net.Http.Json;
using MeuSitePessoal.Application.Articles.Commands.CreateArticle;
using MeuSitePessoal.Application.Articles.Queries.GetArticles;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;

namespace MeuSitePessoal.Tests.Integration.Articles;

/// <summary>
/// HC: Integration tests for the related articles recommendation functionality.
/// </summary>
public class RelatedArticlesTests : BaseIntegrationTest
{
    [Fact]
    public async Task GetRelated_WithSharedTags_ShouldReturnRelevantArticles()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Base article
        var baseCmd = new CreateArticleCommand("Base", "Base", "Content", "Conteúdo", "Sum", "Resumo", ArticleCategory.Technology, new List<string> { "dotnet", "csharp" });
        var baseResp = await _client.PostAsJsonAsync("/api/articles", baseCmd);
        var baseId = await baseResp.Content.ReadFromJsonAsync<Guid>();

        // Related article (shares 'dotnet')
        var relatedCmd = new CreateArticleCommand("Related", "Relacionado", "Content", "Conteúdo", "Sum", "Resumo", ArticleCategory.Technology, new List<string> { "dotnet", "testing" });
        await _client.PostAsJsonAsync("/api/articles", relatedCmd);

        // Unrelated article (shares nothing)
        var unrelatedCmd = new CreateArticleCommand("Unrelated", "Não Relacionado", "Content", "Conteúdo", "Sum", "Resumo", ArticleCategory.Technology, new List<string> { "java", "spring" });
        await _client.PostAsJsonAsync("/api/articles", unrelatedCmd);

        // Act
        var response = await _client.GetAsync($"/api/articles/{baseId}/related");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<ArticleSummaryDto>>();
        
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, a => a.Title == "Related");
        Assert.DoesNotContain(result, a => a.Title == "Unrelated");
        Assert.DoesNotContain(result, a => a.Id == baseId); // Should not contain itself
    }

    [Fact]
    public async Task GetRelated_WithMultipleSharedTags_ShouldOrderArticlesByIntersectionCount()
    {
        // Arrange
        await AuthenticateAsync();
        
        var baseCmd = new CreateArticleCommand("Base", "Base", "Content", "Conteúdo", "Sum", "Resumo", ArticleCategory.Technology, new List<string> { "tag1", "tag2", "tag3" });
        var baseResp = await _client.PostAsJsonAsync("/api/articles", baseCmd);
        var baseId = await baseResp.Content.ReadFromJsonAsync<Guid>();

        // High relevance (2 tags)
        var highRelCmd = new CreateArticleCommand("High", "Alto", "Content", "Conteúdo", "Sum", "Resumo", ArticleCategory.Technology, new List<string> { "tag1", "tag2" });
        await _client.PostAsJsonAsync("/api/articles", highRelCmd);

        // Low relevance (1 tag)
        var lowRelCmd = new CreateArticleCommand("Low", "Baixo", "Content", "Conteúdo", "Sum", "Resumo", ArticleCategory.Technology, new List<string> { "tag1" });
        await _client.PostAsJsonAsync("/api/articles", lowRelCmd);

        // Act
        var response = await _client.GetAsync($"/api/articles/{baseId}/related");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<ArticleSummaryDto>>();
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("High", result[0].Title); // Most relevant first
        Assert.Equal("Low", result[1].Title);
    }
}