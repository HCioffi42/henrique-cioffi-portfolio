using FluentAssertions;
using MeuSitePessoal.Api.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Api.Configuration;

/// <summary>
/// Covers the binding and validation of request-throttling thresholds.
/// Thresholds are configuration rather than literals, so a missing or malformed entry must be
/// caught while the application starts rather than on the first request to an endpoint that
/// nothing happens to exercise.
/// </summary>
public class RateLimitingOptionsTests
{
    /// <summary>
    /// Builds a configuration carrying a complete set of policies, optionally omitting one.
    /// </summary>
    private static IConfiguration BuildConfiguration(string? omitPolicy = null, int permitLimit = 10)
    {
        var values = new Dictionary<string, string?>();

        foreach (var policy in RateLimitPolicies.All)
        {
            if (policy == omitPolicy)
            {
                continue;
            }

            values[$"RateLimiting:Policies:{policy}:PermitLimit"] = permitLimit.ToString();
            values[$"RateLimiting:Policies:{policy}:WindowMinutes"] = "15";
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    [Fact]
    public void ReadAndValidate_WithMissingPolicyEntry_ThrowsNamingThePolicy()
    {
        var configuration = BuildConfiguration(omitPolicy: RateLimitPolicies.CommentsWrite);

        var act = () => RateLimiterSetup.ReadAndValidate(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{RateLimitPolicies.CommentsWrite}*");
    }

    [Fact]
    public void ReadAndValidate_WithAllPoliciesPresent_Succeeds()
    {
        var options = RateLimiterSetup.ReadAndValidate(BuildConfiguration());

        options.Policies.Should().ContainKeys(RateLimitPolicies.All);
    }

    [Fact]
    public void ReadAndValidate_BindsThresholdsFromConfiguration()
    {
        var options = RateLimiterSetup.ReadAndValidate(BuildConfiguration(permitLimit: 42));

        var policy = options.Policies[RateLimitPolicies.AuthSignIn];
        policy.PermitLimit.Should().Be(42);
        policy.WindowMinutes.Should().Be(15);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReadAndValidate_WithNonPositivePermitLimit_ThrowsNamingThePolicy(int permitLimit)
    {
        // A zero permit limit would reject every request to the endpoint, and a negative one is
        // meaningless. Both are configuration faults rather than deliberate policy.
        var configuration = BuildConfiguration(permitLimit: permitLimit);

        var act = () => RateLimiterSetup.ReadAndValidate(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{RateLimitPolicies.AuthSignIn}*");
    }

    [Fact]
    public void ReadAndValidate_WithNoSectionAtAll_ThrowsNamingTheFirstMissingPolicy()
    {
        var configuration = new ConfigurationBuilder().Build();

        var act = () => RateLimiterSetup.ReadAndValidate(configuration);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void All_ContainsEveryDeclaredPolicyName()
    {
        // Guards against a constant being added without being registered for validation,
        // which would leave an endpoint annotated with a policy nothing checks.
        RateLimitPolicies.All.Should().BeEquivalentTo(new[]
        {
            RateLimitPolicies.AuthSignIn,
            RateLimitPolicies.AuthVerify,
            RateLimitPolicies.AuthRegister,
            RateLimitPolicies.AuthRecovery,
            RateLimitPolicies.CommentsWrite,
            RateLimitPolicies.NewsletterSubscribe
        });
    }
}
