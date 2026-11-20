using FluentAssertions;
using MeuSitePessoal.Application.Common.Models.Email;
using MeuSitePessoal.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Infrastructure.Services;

/// <summary>
/// Renders the real NewPostNotification template through the production Razor engine.
/// Templates are copied to the output directory, so no fixture setup is required.
/// These tests exercise the rendering path that previously shipped blank emails.
/// </summary>
public class NewPostNotificationTemplateTests
{
    private const string TemplateName = "Email/NewPostNotification";
    private const string ArticleUrl = "https://hcioffi.dev/article/abc";
    private const string UnsubscribeUrl = "https://hcioffi.dev/api/newsletter/unsubscribe?token=xyz";

    private static RazorEmailTemplateService CreateService() =>
        new(NullLogger<RazorEmailTemplateService>.Instance);

    [Fact]
    public async Task Render_WithBothLanguages_ContainsAllFourStrings()
    {
        var model = new NewPostNotificationViewModel(
            "Getting Started", "An English summary.",
            "Primeiros Passos", "Um resumo em portugues.",
            ArticleUrl, UnsubscribeUrl);

        var html = await CreateService().RenderTemplateAsync(TemplateName, model);

        html.Should().Contain("Getting Started");
        html.Should().Contain("An English summary.");
        html.Should().Contain("Primeiros Passos");
        html.Should().Contain("Um resumo em portugues.");
        html.Should().Contain("Read Full Article");
        html.Should().Contain("Ler Artigo Completo");
    }

    [Fact]
    public async Task Render_WithOnlyEnglish_OmitsPortugueseBlock()
    {
        var model = new NewPostNotificationViewModel(
            "Getting Started", "An English summary.",
            null, null,
            ArticleUrl, UnsubscribeUrl);

        var html = await CreateService().RenderTemplateAsync(TemplateName, model);

        html.Should().Contain("Getting Started");
        html.Should().NotContain("Ler Artigo Completo");
    }

    [Fact]
    public async Task Render_EscapesHtmlSpecialCharactersInTitle()
    {
        var model = new NewPostNotificationViewModel(
            "Tips & <Tricks>", "An English summary.",
            "Dicas e Truques", "Um resumo em portugues.",
            ArticleUrl, UnsubscribeUrl);

        var html = await CreateService().RenderTemplateAsync(TemplateName, model);

        html.Should().Contain("Tips &amp; &lt;Tricks&gt;");
        html.Should().NotContain("<Tricks>");
    }

    [Fact]
    public async Task Render_AlwaysContainsUnsubscribeUrl()
    {
        var model = new NewPostNotificationViewModel(
            "Getting Started", "An English summary.",
            "Primeiros Passos", "Um resumo em portugues.",
            ArticleUrl, UnsubscribeUrl);

        var html = await CreateService().RenderTemplateAsync(TemplateName, model);

        html.Should().Contain(UnsubscribeUrl);
        html.Should().Contain(ArticleUrl);
    }
}
