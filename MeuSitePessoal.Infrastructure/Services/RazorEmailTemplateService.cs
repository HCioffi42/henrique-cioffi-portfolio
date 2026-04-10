using MeuSitePessoal.Application.Common.Interfaces;
using RazorLight;
using PreMailer.Net;
using Microsoft.Extensions.Logging;

namespace MeuSitePessoal.Infrastructure.Services;

/// <summary>
/// Implements the <see cref="IEmailTemplateService"/> using RazorLight for rendering and PreMailer.Net for CSS inlining.
/// </summary>
public class RazorEmailTemplateService : IEmailTemplateService
{
    private readonly IRazorLightEngine _engine;
    private readonly ILogger<RazorEmailTemplateService> _logger;

    /// <summary>
    /// Initializes a new instance of the service, configuring the RazorLight engine.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public RazorEmailTemplateService(ILogger<RazorEmailTemplateService> logger)
    {
        _logger = logger;

        // HC: Configure RazorLight to look for templates in the output directory's Templates/Email folder.
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "Email");

        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(templatePath)
            .UseMemoryCachingProvider()
            .Build();
    }

    /// <summary>
    /// Renders a template and automatically inlines the CSS for maximum email client compatibility.
    /// </summary>
    public async Task<string> RenderTemplateAsync<T>(string templateName, T model)
    {
        try
        {
            // HC: Append .cshtml if not present (RazorLight typically expects the key, 
            // but FileSystemProject might need the full filename depending on configuration).
            var templateKey = templateName.EndsWith(".cshtml") ? templateName : $"{templateName}.cshtml";

            // 1. Render the Razor template to HTML
            string html = await _engine.CompileRenderAsync(templateKey, model);

            // 2. Perform CSS inlining using PreMailer.Net
            var result = PreMailer.Net.PreMailer.MoveCssInline(html);
            
            _logger.LogInformation("Successfully rendered template '{TemplateName}' with CSS inlining.", templateName);
            
            return result.Html;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering email template '{TemplateName}'.", templateName);
            throw;
        }
    }
}
