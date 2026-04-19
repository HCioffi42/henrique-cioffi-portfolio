namespace MeuSitePessoal.Application.Common.Interfaces;

/// <summary>
/// Provides the current language of the request based on headers or user preferences.
/// </summary>
public interface ILanguageProvider
{
    /// <summary>
    /// Gets the current language code (e.g., 'en', 'pt').
    /// </summary>
    string GetCurrentLanguage();
}
