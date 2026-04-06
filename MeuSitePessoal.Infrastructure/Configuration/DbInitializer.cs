using MeuSitePessoal.Domain;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Infrastructure.Configuration;

/// <summary>
/// Responsible for seeding the database with initial data if it is empty.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(BlogDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // 1. Seed Roles
        if (!await context.Roles.AnyAsync())
        {
            Console.WriteLine("--> Seed: Creating Roles...");
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // 2. Seed Admin User
        if (!await context.Users.AnyAsync())
        {
            Console.WriteLine("--> Seed: Creating Admin User...");
            var adminUser = new IdentityUser { UserName = "admin", Email = "admin@meusitepessoal.com", EmailConfirmed = true };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("--> Seed: Admin User created successfully!");
            }
            else 
            {
                Console.WriteLine("--> SEED ERROR: Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // 3. Seed Articles
        if (await context.Articles.AnyAsync()) 
        {
            Console.WriteLine("--> Seed: Articles already exist. Skipping...");
            return;
        }

        Console.WriteLine("--> Seed: Creating Initial Articles...");

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
        Console.WriteLine("--> Seed: Articles saved to database!");
    }
}