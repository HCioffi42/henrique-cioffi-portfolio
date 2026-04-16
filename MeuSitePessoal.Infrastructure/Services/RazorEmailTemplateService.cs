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

        // HC: Sets the root path to the "Templates" folder instead of "Templates/Email".
        // This allows the engine to resolve subfolders like /Email/ or /Auth/ correctly.
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates");

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
            // HC: Normalizes the template key. If the name starts with a slash, it removes it
            // to prevent resolution errors in some file systems.
            var templateKey = templateName.StartsWith("/") ? templateName : $"/{templateName}";
            if (!templateKey.EndsWith(".cshtml"))
            {
                templateKey += ".cshtml";
            }

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
