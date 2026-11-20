using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace MeuSitePessoal.Api.Configuration;

/// <summary>
/// Registers the per-endpoint request budgets.
/// </summary>
public static class RateLimiterSetup
{
    /// <summary>
    /// The partition every request falls into when no client address can be resolved.
    /// Deployed traffic always carries one, because the reverse proxy is the only ingress;
    /// an in-memory test host has no connection and lands here.
    /// </summary>
    private const string UnresolvedClientPartition = "unresolved-client";

    /// <summary>
    /// Reads the thresholds and verifies that every declared policy has a usable entry.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a policy is absent or carries a non-positive threshold. An endpoint annotated
    /// with a policy the limiter never registered fails on the first request that reaches it,
    /// which may be long after deployment, so this is checked while the application starts.
    /// </exception>
    public static RateLimitingOptions ReadAndValidate(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var options = configuration.GetSection(RateLimitingOptions.SectionName)
                          .Get<RateLimitingOptions>()
                      ?? new RateLimitingOptions();

        foreach (var policyName in RateLimitPolicies.All)
        {
            if (!options.Policies.TryGetValue(policyName, out var policy))
            {
                throw new InvalidOperationException(
                    $"Rate limiting policy '{policyName}' has no entry under " +
                    $"'{RateLimitingOptions.SectionName}:Policies'. Every declared policy must be " +
                    "configured, otherwise the endpoints using it are left unprotected.");
            }

            if (policy.PermitLimit <= 0 || policy.WindowMinutes <= 0)
            {
                throw new InvalidOperationException(
                    $"Rate limiting policy '{policyName}' is misconfigured. PermitLimit and " +
                    "WindowMinutes must both be greater than zero.");
            }
        }

        return options;
    }

    /// <summary>
    /// Validates the configured thresholds and registers one fixed-window policy per endpoint
    /// budget, partitioned by the resolved client address.
    /// </summary>
    public static IServiceCollection AddConfiguredRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = ReadAndValidate(configuration);

        services.AddRateLimiter(limiter =>
        {
            // The framework default is 503, which would present throttling as an outage to
            // monitoring and to any client distinguishing the two.
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            foreach (var policyName in RateLimitPolicies.All)
            {
                var policy = options.Policies[policyName];

                limiter.AddPolicy(policyName, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        ResolvePartitionKey(httpContext),
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = policy.PermitLimit,
                            Window = TimeSpan.FromMinutes(policy.WindowMinutes),
                            // Requests over budget are refused outright rather than delayed.
                            QueueLimit = 0,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            AutoReplenishment = true
                        }));
            }

            limiter.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("MeuSitePessoal.Api.RateLimiting");

                logger.LogWarning(
                    "Request budget exhausted for {Path} from partition {Partition}.",
                    context.HttpContext.Request.Path,
                    ResolvePartitionKey(context.HttpContext));

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                // The body states nothing about thresholds, windows, or remaining budget.
                await context.HttpContext.Response.WriteAsync(
                    "Too many requests. Please try again later.", cancellationToken);
            };
        });

        return services;
    }

    /// <summary>
    /// Identifies the caller a budget is charged against. This is the address resolved by the
    /// forwarded-headers middleware, which must run earlier in the pipeline; without it every
    /// caller behind the proxy would share one partition.
    /// </summary>
    private static string ResolvePartitionKey(HttpContext httpContext)
        => httpContext.Connection.RemoteIpAddress?.ToString() ?? UnresolvedClientPartition;
}
