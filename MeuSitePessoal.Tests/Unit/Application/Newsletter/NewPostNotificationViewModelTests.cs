using FluentAssertions;
using MeuSitePessoal.Application.Common.Models.Email;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Newsletter;

/// <summary>
/// Covers the subject composition and language-block visibility rules defined in
/// the Track 17.5 specification, section 4.1 and 4.3.
/// </summary>
public class NewPostNotificationViewModelTests
{
    private const string ArticleUrl = "https://hcioffi.dev/article/abc";
    private const string UnsubscribeUrl = "https://hcioffi.dev/api/newsletter/unsubscribe";

    private static NewPostNotificationViewModel Build(
        string? titleEn, string? summaryEn, string? titlePt, string? summaryPt) =>
        new(titleEn, summaryEn, titlePt, summaryPt, ArticleUrl, UnsubscribeUrl);

    [Fact]
    public void Subject_WithDistinctTitles_JoinsWithPipe()
    {
        var model = Build("Hello", "summary", "Olá", "resumo");

        model.Subject.Should().Be("Hello | Olá");
        model.ShowEnglish.Should().BeTrue();
        model.ShowPortuguese.Should().BeTrue();
    }

    [Fact]
    public void Subject_WithIdenticalTitles_CollapsesToOne()
    {
        // The Track 17 backfill copied the single legacy title into both localized
        // columns, so every pre-migration article carries identical titles.
        var model = Build("Hello", "summary", "Hello", "resumo");

        model.Subject.Should().Be("Hello");
    }

    [Fact]
    public void Subject_WithOnlyEnglishPopulated_UsesEnglishTitle()
    {
        var model = Build("Hello", "summary", null, null);

        model.Subject.Should().Be("Hello");
        model.ShowPortuguese.Should().BeFalse();
        model.HasContent.Should().BeTrue();
    }

    [Fact]
    public void Subject_WithOnlyPortuguesePopulated_UsesPortugueseTitle()
    {
        var model = Build(null, null, "Olá", "resumo");

        model.Subject.Should().Be("Olá");
        model.ShowEnglish.Should().BeFalse();
        model.HasContent.Should().BeTrue();
    }

    [Fact]
    public void HasContent_WithBothLanguagesEmpty_IsFalse()
    {
        var model = Build(null, null, null, null);

        model.HasContent.Should().BeFalse();
        model.Subject.Should().BeEmpty();
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("")]
    public void ShowEnglish_WithWhitespaceOnlyTitle_IsFalse(string title)
    {
        var model = Build(title, "summary", "Olá", "resumo");

        model.ShowEnglish.Should().BeFalse();
        model.Subject.Should().Be("Olá");
    }

    [Fact]
    public void ShowEnglish_WithTitleButEmptySummary_IsFalse()
    {
        // A half-populated language is suppressed entirely rather than rendered
        // with a missing summary.
        var model = Build("Hello", "", "Olá", "resumo");

        model.ShowEnglish.Should().BeFalse();
        model.Subject.Should().Be("Olá");
    }
}
