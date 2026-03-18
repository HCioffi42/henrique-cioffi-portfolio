using MeuSitePessoal.Domain;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace MeuSitePessoal.Infrastructure.Configuration;

/// <summary>
/// Responsible for seeding the database with initial data if it is empty.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(BlogDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // 1. Seed Roles
        if (!context.Roles.Any())
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // 2. Seed Admin User
        if (!context.Users.Any())
        {
            var adminUser = new IdentityUser
            {
                UserName = "admin",
                Email = "admin@meusitepessoal.com",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // 3. Seed Artigos
        if (context.Artigos.Any()) return;

        var artigosIniciais = new List<Artigo>
        {
            new Artigo(
                "Iniciando com .NET 8 e C#", 
                "Este é o conteúdo do meu primeiro artigo técnico sobre a plataforma .NET.", 
                "Um guia introdutório ao ecossistema .NET moderno.", 
                new List<string> { ".net", "csharp", "backend" }),
            
            new Artigo(
                "PostgreSQL no Docker", 
                "Aprenda a subir um container do Postgres de forma rápida para seus testes.", 
                "Configuração rápida de ambiente de banco de dados.", 
                new List<string> { "docker", "database", "postgres" }),
            
            new Artigo(
                "Arquitetura Limpa na Prática", 
                "Como organizar suas camadas para manter um código sustentável a longo prazo.", 
                "Dicas de organização de projetos ASP.NET Core.", 
                new List<string> { "architecture", "clean-code" })
        };

        await context.Artigos.AddRangeAsync(artigosIniciais);
        await context.SaveChangesAsync();
    }
}