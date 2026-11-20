using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeuSitePessoal.Infrastructure.Configuration;

/// <summary>
/// Responsible for seeding the database with initial data and identity infrastructure.
/// Refactored for security and robust role management.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Seeds roles, admin user, and initial articles into the database.
    /// </summary>
    public static async Task SeedAsync(
        BlogDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        ILogger logger)
    {
        logger.LogInformation("--> Seed: Starting identity infrastructure seeding...");

        // 1. Seed Roles
        var roles = new[] { "Admin", "Reader" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                logger.LogInformation("--> Seed: Creating {Role} role...", role);
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed/Verify Admin User from Configuration
        var adminEmail = configuration["AdminSetup:Email"];
        var adminPassword = configuration["AdminSetup:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("--> Seed: Admin credentials not configured in AdminSetup section. Skipping admin seeding.");
        }
        else
        {
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                logger.LogInformation("--> Seed: Admin user {Email} not found. Creating...", adminEmail);
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PreferredLanguage = "en"
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    logger.LogInformation("--> Seed: Admin user created successfully.");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogError("--> Seed: Failed to create admin user: {Errors}", errors);
                    adminUser = null; // Ensure we don't try to add to role if it failed
                }
            }

            // Ensure the admin user is in the Admin role
            if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                logger.LogInformation("--> Seed: Promoting user {Email} to Admin role...", adminEmail);
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Converges the administrative account's lockout exemption on every startup. This sits
            // outside the role conditional above on purpose: the deployed administrator is already
            // in the role, so the promotion branch never runs again, while every integration test
            // creates the account fresh and always takes the creation branch. An exemption attached
            // to either branch would pass the suite and leave the production account lockable.
            if (adminUser != null)
            {
                var lockoutStateChanged = false;

                if (adminUser.LockoutEnabled)
                {
                    adminUser.LockoutEnabled = false;
                    lockoutStateChanged = true;
                }

                // AccessFailedAsync writes the two fields below without consulting LockoutEnabled,
                // so an exempt account accumulates lockout state that reads as a locked row today
                // and would take effect the moment the exemption were lifted.
                if (adminUser.LockoutEnd != null)
                {
                    adminUser.LockoutEnd = null;
                    lockoutStateChanged = true;
                }

                if (adminUser.AccessFailedCount != 0)
                {
                    adminUser.AccessFailedCount = 0;
                    lockoutStateChanged = true;
                }

                // Written only when something differs, so an ordinary boot performs no write.
                if (lockoutStateChanged)
                {
                    logger.LogInformation(
                        "--> Seed: Converging lockout exemption for administrator {Email}.", adminEmail);
                    await userManager.UpdateAsync(adminUser);
                }
            }
        }

        // 3. Seed Articles
        if (await context.Articles.AnyAsync()) 
        {
            logger.LogInformation("--> Seed: Articles already exist. Skipping article seeding.");
            return;
        }

        logger.LogInformation("--> Seed: Creating initial articles...");

        var initialArticles = new List<Article>
        {
            new Article(
                "Getting started with .NET 8 and C#", 
                "Iniciando com .NET 8 e C#",
                "This is the content of my first technical article about the .NET platform.", 
                "Este é o conteúdo do meu primeiro artigo técnico sobre a plataforma .NET.",
                "An introductory guide to the modern .NET ecosystem.", 
                "Um guia introdutório ao ecossistema .NET moderno.",
                new List<string> { ".net", "csharp", "backend" },
                ArticleCategory.Technology),
            
            new Article(
                "PostgreSQL on Docker", 
                "PostgreSQL no Docker",
                "Learn how to quickly spin up a Postgres container for your tests.", 
                "Aprenda como subir rapidamente um container Postgres para seus testes.",
                "Quick database environment setup.", 
                "Configuração rápida de ambiente de banco de dados.",
                new List<string> { "docker", "database", "postgres" },
                ArticleCategory.Technology),
            
            new Article(
                "Clean Architecture in Practice", 
                "Clean Architecture na Prática",
                "How to organize your layers to maintain sustainable long-term code.", 
                "Como organizar suas camadas para manter um código sustentável a longo prazo.",
                "Tips for organizing ASP.NET Core projects.", 
                "Dicas para organizar projetos ASP.NET Core.",
                new List<string> { "architecture", "clean-code" },
                ArticleCategory.Technology),
            
            new Article(
                "Welcome to my new Blog",
                "Bem-vindo ao meu novo Blog",
                "This is the first article after the English refactoring.",
                "Este é o primeiro artigo após o refactoring para inglês.",
                "A fresh start with clean code.",
                "Um novo começo com código limpo.",
                new List<string> { "dotnet", "clean-code" },
                ArticleCategory.News)
        };

        await context.Articles.AddRangeAsync(initialArticles);
        await context.SaveChangesAsync();
        logger.LogInformation("--> Seed: Initial articles saved successfully.");
    }
}
