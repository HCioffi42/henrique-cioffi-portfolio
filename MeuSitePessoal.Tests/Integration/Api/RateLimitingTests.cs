using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeuSitePessoal.Api.Configuration;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Api;

/// <summary>
/// Covers the per-endpoint request budgets.
/// Each test method builds its own host, so limiter state never carries between tests.
/// Thresholds are deliberately small here; the suite's defaults are high enough that ordinary
/// tests never reach them.
/// </summary>
public class RateLimitingTests : BaseIntegrationTest
{
    private const int TestPermitLimit = 2;

    /// <summary>
    /// Shrinks every budget so a handful of requests exhausts it, while keeping the window long
    /// enough that it cannot replenish mid-test.
    /// </summary>
    protected override IDictionary<string, string?> AdditionalConfiguration
    {
        get
        {
            var values = new Dictionary<string, string?>();

            foreach (var policy in RateLimitPolicies.All)
            {
                values[$"RateLimiting:Policies:{policy}:PermitLimit"] = TestPermitLimit.ToString();
                values[$"RateLimiting:Policies:{policy}:WindowMinutes"] = "60";
            }

            return values;
        }
    }

    private Task<HttpResponseMessage> PostAsync(string path, string? forwardedFor = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(new { })
        };

        if (forwardedFor is not null)
        {
            request.Headers.Add("X-Forwarded-For", forwardedFor);
        }

        return _client.SendAsync(request);
    }

    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/register")]
    [InlineData("/api/auth/forgot-password")]
    [InlineData("/api/auth/reset-password")]
    [InlineData("/api/newsletter/subscribe")]
    public async Task Endpoint_ReturnsTooManyRequests_OnceBudgetIsExhausted(string path)
    {
        for (var i = 0; i < TestPermitLimit; i++)
        {
            await PostAsync(path);
        }

        var rejected = await PostAsync(path);

        // Asserts the status specifically. The framework's default rejection status is 503,
        // which would present throttling as an outage; "not successful" would not catch that.
        rejected.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Comments_ReturnTooManyRequests_OnceBudgetIsExhausted()
    {
        await AuthenticateAsync();

        for (var i = 0; i < TestPermitLimit; i++)
        {
            await PostAsync("/api/comments");
        }

        var rejected = await PostAsync("/api/comments");

        rejected.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task RequestWithinBudget_ReturnsNormalStatus()
    {
        // Proves the limiter rejects selectively rather than refusing everything. The endpoint
        // answers on its own terms; what matters is that throttling is not what answered.
        var response = await PostAsync("/api/auth/login");

        response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task DistinctForwardedAddresses_ConsumeSeparateBudgets()
    {
        // The assertion that makes forwarded-header resolution load-bearing: without it every
        // caller behind the proxy would share one budget.
        for (var i = 0; i < TestPermitLimit; i++)
        {
            await PostAsync("/api/auth/login", forwardedFor: "203.0.113.9");
        }

        var sameAddress = await PostAsync("/api/auth/login", forwardedFor: "203.0.113.9");
        var otherAddress = await PostAsync("/api/auth/login", forwardedFor: "198.51.100.4");

        sameAddress.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        otherAddress.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task RejectedResponse_CarriesRetryAfter()
    {
        for (var i = 0; i < TestPermitLimit; i++)
        {
            await PostAsync("/api/auth/login");
        }

        var rejected = await PostAsync("/api/auth/login");

        rejected.Headers.RetryAfter.Should().NotBeNull();
    }

    [Fact]
    public async Task SecretVerification_DoesNotConsumeTheSignInBudget()
    {
        // Pins the separation of auth-verify from auth-signin. Sharing one policy would let a
        // caller exhausting reset attempts lock out sign-in for everyone on the same address.
        for (var i = 0; i < TestPermitLimit; i++)
        {
            await PostAsync("/api/auth/reset-password");
        }

        var verifyRejected = await PostAsync("/api/auth/reset-password");
        var signInStillAllowed = await PostAsync("/api/auth/login");

        verifyRejected.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        signInStillAllowed.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
    }
}
