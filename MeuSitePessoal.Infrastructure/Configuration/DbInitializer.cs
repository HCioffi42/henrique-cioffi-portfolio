using MeuSitePessoal.Domain;
using MeuSitePessoal.Infrastructure.Data;

namespace MeuSitePessoal.Infrastructure.Configuration;

// Responsável por popular o banco de dados com dados iniciais caso ele esteja vazio.
public static class DbInitializer
{
    public static async Task SeedAsync(BlogDbContext context)
    {
        // Verifica se já existem artigos para evitar duplicidade.
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