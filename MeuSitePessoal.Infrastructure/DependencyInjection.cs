using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Infrastructure.Data;
using MeuSitePessoal.Infrastructure.Configuration;
using MeuSitePessoal.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeuSitePessoal.Infrastructure;

/// <summary>
/// Centralizes the registration of all infrastructure layer dependencies.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Retrieves the connection string and adds a validation check to prevent null reference errors during startup.
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? throw new InvalidOperationException("The ConnectionString 'DefaultConnection' was not found in the configuration.");
        
        // Configures the DbContext to use PostgreSQL and ensures migrations are generated within the Infrastructure project.
        services.AddDbContext<BlogDbContext>(options =>
            options.UseNpgsql(connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(BlogDbContext).Assembly.FullName)));

        // Binds the abstract database interface to its concrete EF Core implementation, keeping the Application layer agnostic.
        services.AddScoped<IBlogDbContext>(provider => provider.GetRequiredService<BlogDbContext>());
        
        // Registers the specialized search service to handle full-text search capabilities for articles.
        services.AddScoped<IArticleSearchService, ArticleSearchService>();

        // Configures email settings from the configuration section and registers both template and sending (MailKit) services.
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        
        // Uses a Singleton for the RazorLightEngine to leverage the template compilation cache and optimize performance.
        services.AddSingleton<IEmailTemplateService, RazorEmailTemplateService>();
        services.AddTransient<IEmailSender, MailKitEmailService>();

        return services;
    }
}
