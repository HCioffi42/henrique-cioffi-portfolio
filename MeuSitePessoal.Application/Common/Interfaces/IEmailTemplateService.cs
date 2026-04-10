namespace MeuSitePessoal.Application.Common.Interfaces;

/// <summary>
/// Provides services for rendering professional email templates using Razor syntax.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Renders a Razor template with the specified model and returns the HTML string with inlined CSS.
    /// </summary>
    /// <typeparam name="T">The type of the view model.</typeparam>
    /// <param name="templateName">The name of the template (without extension).</param>
    /// <param name="model">The data model to be passed to the template.</param>
    /// <returns>A task that represents the asynchronous operation, containing the rendered HTML string.</returns>
    Task<string> RenderTemplateAsync<T>(string templateName, T model);
}
