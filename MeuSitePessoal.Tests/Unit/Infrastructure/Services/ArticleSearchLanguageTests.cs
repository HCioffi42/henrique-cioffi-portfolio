using FluentAssertions;
using MeuSitePessoal.Infrastructure.Services;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Pins language resolution at the layer that decides which dictionary and which columns
/// a search reads. The HTTP provider currently normalizes every Portuguese header to the
/// literal "pt", so an end-to-end test cannot distinguish an ordinal comparison from a
/// case-insensitive one. These assertions exercise the predicate directly, which keeps the
/// guarantee intact if the provider ever starts returning a richer tag such as "pt-BR".
/// </summary>
public class ArticleSearchLanguageTests
{
    [Theory]
    [InlineData("pt")]
    [InlineData("pt-BR")]
    [InlineData("pt_BR")]
    [InlineData("PT")]
    [InlineData("PT-BR")]
    [InlineData("Pt-br")]
    [InlineData("pt-PT")]
    public void IsPortuguese_ResolvesTrue_ForCaseAndRegionVariants(string language) =>
        ArticleSearchService.IsPortuguese(language).Should().BeTrue();

    [Theory]
    [InlineData("en")]
    [InlineData("en-US")]
    [InlineData("EN")]
    [InlineData("es")]
    [InlineData("fr-FR")]
    [InlineData("")]
    public void IsPortuguese_ResolvesFalse_ForEverythingElse(string language) =>
        ArticleSearchService.IsPortuguese(language).Should().BeFalse();

    [Fact]
    public void IsPortuguese_ResolvesFalse_ForNull() =>
        ArticleSearchService.IsPortuguese(null).Should().BeFalse();
}
