using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MeuSitePessoal.Infrastructure.Logging;

/**
 * Dependency Injection class for configuring Serilog logging.
 * This class isolates logging infrastructure from the rest of the application.
 */
public static class DependencyInjection
{
    /**
     * Extension method to add custom Serilog configuration to the IServiceCollection.
     * @param services The IServiceCollection to add the services to.
     * @param configuration The IConfiguration to read settings from.
     * @returns The updated IServiceCollection.
     */
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
}
