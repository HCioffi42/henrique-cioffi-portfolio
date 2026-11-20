using FluentAssertions;
using MeuSitePessoal.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeuSitePessoal.Tests.Integration.Api;

/// <summary>
/// Covers the resolution of the originating client address when the application runs behind a
/// reverse proxy. The resolved address is not observable from any production response, so these
/// tests append a terminal middleware that records it for a path no controller route matches.
/// </summary>
public class ForwardedHeadersTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly RemoteAddressSink _sink = new();

    private const string ProbePath = "/__test/remote-address";

    static ForwardedHeadersTests()
    {
        TestHostEnvironment.Apply();
    }

    public ForwardedHeadersTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(_sink);
                    services.AddSingleton<IStartupFilter, RemoteAddressCaptureFilter>();
                });
            });

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task RemoteIpAddress_ReflectsForwardedHeader_WhenSingleHopPresented()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, ProbePath);
        request.Headers.Add("X-Forwarded-For", "203.0.113.9");

        await _client.SendAsync(request);

        _sink.LastRemoteAddress.Should().Be("203.0.113.9");
    }

    [Fact]
    public async Task RemoteIpAddress_IgnoresSecondHop_WhenChainExceedsTrustedLimit()
    {
        // A caller may prepend entries to the chain. Trusting a single hop means the address
        // attributed is the nearest one, not whatever the caller placed at the front.
        var request = new HttpRequestMessage(HttpMethod.Get, ProbePath);
        request.Headers.Add("X-Forwarded-For", "198.51.100.4, 203.0.113.9");

        await _client.SendAsync(request);

        _sink.LastRemoteAddress.Should().Be("203.0.113.9");
        _sink.LastRemoteAddress.Should().NotBe("198.51.100.4");
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>Holds the address observed by the capturing middleware.</summary>
    private sealed class RemoteAddressSink
    {
        public string? LastRemoteAddress { get; set; }
    }

    /// <summary>
    /// Appends the capturing middleware after the application's own pipeline. Calling next first
    /// is what makes this a test of the application's wiring: replacing the pipeline instead would
    /// assert on middleware the test itself registered, and would pass regardless of how the
    /// application is configured.
    /// </summary>
    private sealed class RemoteAddressCaptureFilter : IStartupFilter
    {
        private readonly RemoteAddressSink _sink;

        public RemoteAddressCaptureFilter(RemoteAddressSink sink) => _sink = sink;

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                next(app);

                app.Run(async context =>
                {
                    _sink.LastRemoteAddress = context.Connection.RemoteIpAddress?.ToString();
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    await context.Response.WriteAsync("captured");
                });
            };
        }
    }
}
