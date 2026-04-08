namespace MeuSitePessoal.Application.Common.Interfaces;

/// <summary>
/// Provides access to feature toggle flags defined in application configuration.
/// The service decouples the Application layer from direct IConfiguration dependencies.
/// </summary>
public interface IFeatureToggleService
{
    /// <summary>
    /// Determines whether the Two-Factor Authentication (2FA) challenge is enabled.
    /// </summary>
    /// <returns><c>true</c> if 2FA is required during login; otherwise, <c>false</c>.</returns>
    bool Is2FAEnabled();
}
