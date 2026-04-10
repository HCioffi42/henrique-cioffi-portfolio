using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Infrastructure.Data;
using MeuSitePessoal.Infrastructure.Configuration;
using MeuSitePessoal.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeuSitePessoal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BlogDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(BlogDbContext).Assembly.FullName)));

        services.AddScoped<IBlogDbContext>(provider => provider.GetRequiredService<BlogDbContext>());

        services.AddScoped<IArticleSearchService, ArticleSearchService>();

        // Registers email configuration and service implementation.
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddTransient<IEmailSender, MailKitEmailService>();

        return services;
    }
}