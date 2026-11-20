using System.Text;
using FluentAssertions;
using MeuSitePessoal.Api.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Api.Configuration;

/// <summary>
/// Covers the startup resolution of the JWT signing key, which is supplied by the
/// environment rather than by a committed configuration file.
/// </summary>
public class JwtKeyResolverTests
{
    /// <summary>
    /// Builds a configuration carrying only the signing key, omitting the member entirely
    /// when the supplied value is null so the absent case is genuinely absent.
    /// </summary>
    private static IConfiguration BuildConfiguration(string? key)
    {
        var values = new Dictionary<string, string?>();

        if (key is not null)
        {
            values["JwtSettings:Key"] = key;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_WithMissingKey_ThrowsNamingTheConfigurationPath(string? key)
    {
        var configuration = BuildConfiguration(key);

        var act = () => JwtKeyResolver.Resolve(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JwtSettings:Key*");
    }

    [Fact]
    public void Resolve_WithKeyShorterThanMinimum_Throws()
    {
        // Thirty-one ASCII characters encode to thirty-one bytes, one short of the minimum.
        var configuration = BuildConfiguration(new string('a', JwtKeyResolver.MinimumKeyBytes - 1));

        var act = () => JwtKeyResolver.Resolve(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JwtSettings:Key*");
    }

    [Fact]
    public void Resolve_WithKeyAtExactlyMinimum_Succeeds()
    {
        // The boundary is inclusive: a key of exactly the minimum length is acceptable.
        var configuration = BuildConfiguration(new string('a', JwtKeyResolver.MinimumKeyBytes));

        var key = JwtKeyResolver.Resolve(configuration);

        key.Should().HaveCount(JwtKeyResolver.MinimumKeyBytes);
    }

    [Fact]
    public void Resolve_MeasuresLengthInBytesNotCharacters()
    {
        // Twenty characters, each encoding to three bytes, exceed the minimum in bytes
        // while falling short of it in characters. A character-based check would reject this.
        var multiByteKey = new string('あ', 20);
        Encoding.UTF8.GetByteCount(multiByteKey).Should().BeGreaterThan(JwtKeyResolver.MinimumKeyBytes);
        multiByteKey.Length.Should().BeLessThan(JwtKeyResolver.MinimumKeyBytes);

        var key = JwtKeyResolver.Resolve(BuildConfiguration(multiByteKey));

        key.Should().HaveCount(Encoding.UTF8.GetByteCount(multiByteKey));
    }

    [Fact]
    public void Resolve_ExceptionMessage_DoesNotEchoTheConfiguredValue()
    {
        // A rejected key must not be reproduced in diagnostics, which reach logs and consoles.
        const string distinctiveValue = "distinctive-but-too-short";
        var configuration = BuildConfiguration(distinctiveValue);

        var act = () => JwtKeyResolver.Resolve(configuration);

        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().NotContain(distinctiveValue);
    }
}
