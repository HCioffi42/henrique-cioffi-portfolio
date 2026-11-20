namespace MeuSitePessoal.Application.Common.Models.Email;

/// <summary>
/// Carries the data required to render a bilingual new-article notification email.
/// The view model decouples the template from the Article entity and owns the
/// subject composition and language-block visibility rules, so both can be
/// verified without rendering Razor or dispatching mail.
/// </summary>
public sealed record NewPostNotificationViewModel(
    string? TitleEn,
    string? SummaryEn,
    string? TitlePt,
    string? SummaryPt,
    string ArticleUrl,
    string UnsubscribeUrl)
{
    /// <summary>
    /// Gets a value indicating whether the English block carries enough content to render.
    /// A language is shown only when both its title and its summary are present.
    /// </summary>
    public bool ShowEnglish =>
        !string.IsNullOrWhiteSpace(TitleEn) && !string.IsNullOrWhiteSpace(SummaryEn);

    /// <summary>
    /// Gets a value indicating whether the Portuguese block carries enough content to render.
    /// </summary>
    public bool ShowPortuguese =>
        !string.IsNullOrWhiteSpace(TitlePt) && !string.IsNullOrWhiteSpace(SummaryPt);

    /// <summary>
    /// Gets a value indicating whether at least one language can be rendered.
    /// When this is false the notification is not dispatched at all.
    /// </summary>
    public bool HasContent => ShowEnglish || ShowPortuguese;

    /// <summary>
    /// Gets the email subject. Both titles are joined with a pipe when they differ;
    /// articles migrated from the single-language schema carry identical titles in
    /// both columns and collapse to a single title instead.
    /// </summary>
    public string Subject
    {
        get
        {
            if (ShowEnglish && ShowPortuguese && !string.Equals(TitleEn, TitlePt, StringComparison.Ordinal))
                return $"{TitleEn} | {TitlePt}";

            if (ShowEnglish)
                return TitleEn!;

            return ShowPortuguese ? TitlePt! : string.Empty;
        }
    }
}
