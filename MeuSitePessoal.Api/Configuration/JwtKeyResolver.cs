using System.Text;

namespace MeuSitePessoal.Api.Configuration;

/// <summary>
/// Resolves the JWT signing key from configuration during startup.
/// The key is supplied by the environment rather than by a committed configuration file,
/// so its absence is a deployment fault that must stop the application rather than surface
/// later as an obscure failure at the point a token is signed or validated.
/// </summary>
public static class JwtKeyResolver
{
    /// <summary>
    /// The configuration path the key is read from. Environment variables express it as
    /// <c>JwtSettings__Key</c>.
    /// </summary>
    public const string ConfigurationPath = "JwtSettings:Key";

    /// <summary>
    /// The shortest acceptable key, in bytes. HMAC-SHA256 produces a 256-bit signature and
    /// requires a key at least as long; a shorter one is rejected by the signing library at
    /// a far less obvious point.
    /// </summary>
    public const int MinimumKeyBytes = 32;

    /// <summary>
    /// Returns the UTF-8 encoded signing key.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the key is absent, blank, or shorter than <see cref="MinimumKeyBytes"/>.
    /// The message names the configuration path and the accepted sources, and never reproduces
    /// the rejected value, which would otherwise reach logs and consoles.
    /// </exception>
    public static byte[] Resolve(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var configuredValue = configuration[ConfigurationPath];

        if (string.IsNullOrWhiteSpace(configuredValue))
        {
            throw new InvalidOperationException(
                $"The signing key '{ConfigurationPath}' is not configured. Supply it through the " +
                "JwtSettings__Key environment variable in deployed environments, or through " +
                "'dotnet user-secrets set \"JwtSettings:Key\"' for local development.");
        }

        var key = Encoding.UTF8.GetBytes(configuredValue);

        if (key.Length < MinimumKeyBytes)
        {
            throw new InvalidOperationException(
                $"The signing key '{ConfigurationPath}' is too short. It must encode to at least " +
                $"{MinimumKeyBytes} bytes, but the configured value encodes to {key.Length}.");
        }

        return key;
    }
}
