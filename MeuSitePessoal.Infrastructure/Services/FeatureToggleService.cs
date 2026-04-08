using MeuSitePessoal.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MeuSitePessoal.Infrastructure.Services;

/// <summary>
/// Reads feature toggle flags from the application configuration section "FeatureToggles".
/// Implements <see cref="IFeatureToggleService"/> to provide decoupled access to feature flags.
/// </summary>
public class FeatureToggleService : IFeatureToggleService
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of <see cref="FeatureToggleService"/> with the provided configuration.
    /// </summary>
    /// <param name="configuration">The application configuration instance.</param>
    public FeatureToggleService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Reads the <c>FeatureToggles:Enable2FA</c> flag from configuration.
    /// </summary>
    /// <returns><c>true</c> if 2FA is enabled; <c>false</c> otherwise.</returns>
    public bool Is2FAEnabled()
    {
        return _configuration.GetValue<bool>("FeatureToggles:Enable2FA");
    }
}
