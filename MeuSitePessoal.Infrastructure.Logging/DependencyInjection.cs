using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace MeuSitePessoal.Infrastructure.Logging;

/**
 * Dependency Injection class for configuring Serilog logging and OpenTelemetry tracing.
 * This class isolates logging and observability infrastructure from the rest of the application.
 */
public static class DependencyInjection
{
    /// <summary>
    /// Extension method to add custom Serilog configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration provider.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCustomLogging(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            // Enrichment ensures logs contain contextual data like CorrelationId and MachineName.
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            // Configures the Console sink with theme support for better readability during development.
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
            // Configures the File sink with daily rolling and asynchronous writing for performance.
            .WriteTo.Async(a => a.File(
                path: "logs/log-.txt", 
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {MachineName} {Message:lj} {Properties:j}{NewLine}{Exception}"))
            .CreateLogger();

        // Registers Serilog as the logging provider.
        services.AddSerilog();

        return services;
    }

    /**
     * Extension method to configure OpenTelemetry for basic HTTP and SQL tracing.
     * @param services The IServiceCollection to add the services to.
     * @returns The updated IServiceCollection.
     */
    public static IServiceCollection AddCustomTracing(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MeuSitePessoal.Api"))
                    .AddAspNetCoreInstrumentation() // Traces incoming HTTP requests.
                    .AddEntityFrameworkCoreInstrumentation() // Traces database queries.
                    .AddConsoleExporter(); // Simplifies development by showing traces in the console.
            });

        return services;
    }
}
