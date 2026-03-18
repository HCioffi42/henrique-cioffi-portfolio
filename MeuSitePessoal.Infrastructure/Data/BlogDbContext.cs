using MeuSitePessoal.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Infrastructure.Data;

public class BlogDbContext : IdentityDbContext
{
    // Defino o contexto que herda de IdentityDbContext para gerenciar a comunicação com o PostgreSQL e tabelas de Identity.
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }

    // Mapeio a entidade Artigo para uma tabela chamada Artigos no banco de dados.
    public DbSet<Artigo> Artigos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuro a entidade Artigo para garantir que as regras de negócio sejam refletidas no esquema do banco.
        modelBuilder.Entity<Artigo>(builder =>
        {
            builder.ToTable("Artigos");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Titulo).IsRequired().HasMaxLength(200);
            builder.Property(a => a.Conteudo).IsRequired();
            builder.Property(a => a.Resumo).IsRequired().HasMaxLength(500);
            builder.Property(a => a.Tags).IsRequired();
            builder.Property(a => a.DataCriacao).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}