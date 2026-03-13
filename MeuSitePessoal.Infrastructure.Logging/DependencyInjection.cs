using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.File;

namespace MeuSitePessoal.Infrastructure.Logging;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomLogging(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Async(a => a.File("logs/log-.txt", rollingInterval: RollingInterval.Day, shared: true))
            .CreateLogger();

        services.AddSerilog();

        return services;
    }
}
