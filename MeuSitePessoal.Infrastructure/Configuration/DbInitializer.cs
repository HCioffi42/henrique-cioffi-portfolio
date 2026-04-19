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

                var result = await userManager.CreateAsync(adminUser, adminPassword);                if (result.Succeeded)
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
                "This is the content of my first technical article about the .NET platform.", 
                "An introductory guide to the modern .NET ecosystem.", 
                new List<string> { ".net", "csharp", "backend" },
                ArticleCategory.Technology),
            
            new Article(
                "PostgreSQL on Docker", 
                "Learn how to quickly spin up a Postgres container for your tests.", 
                "Quick database environment setup.", 
                new List<string> { "docker", "database", "postgres" },
                ArticleCategory.Technology),
            
            new Article(
                "Clean Architecture in Practice", 
                "How to organize your layers to maintain sustainable long-term code.", 
                "Tips for organizing ASP.NET Core projects.", 
                new List<string> { "architecture", "clean-code" },
                ArticleCategory.Technology),
            
            new Article(
                "Welcome to my new Blog",
                "This is the first article after the English refactoring.",
                "A fresh start with clean code.",
                new List<string> { "dotnet", "clean-code" },
                ArticleCategory.News)
        };

        await context.Articles.AddRangeAsync(initialArticles);
        await context.SaveChangesAsync();
        logger.LogInformation("--> Seed: Initial articles saved successfully.");
    }
}
