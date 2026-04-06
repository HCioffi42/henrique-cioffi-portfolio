using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Infrastructure.Data;

public class BlogDbContext : IdentityDbContext
{
    // Defines the context that inherits from IdentityDbContext to manage communication with PostgreSQL and Identity tables.
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }

    // Maps the Article entity to a table called Articles in the database.
    public DbSet<Article> Articles { get; set; }
    
    // Maps the Subscriber entity to a table called Subscribers in the database.
    public DbSet<Subscriber> Subscribers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // I configure the Article entity to ensure that business rules are reflected in the database schema.
        modelBuilder.Entity<Article>(builder =>
        {
            builder.ToTable("Articles");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
            builder.Property(a => a.Content).IsRequired();
            builder.Property(a => a.Summary).IsRequired().HasMaxLength(500);
            builder.Property(a => a.Tags).IsRequired();
            builder.Property(a => a.Category).IsRequired();
            builder.Property(a => a.CreatedAt).IsRequired();
        });

        // Configures the Subscriber entity with unique index for the email.
        modelBuilder.Entity<Subscriber>(builder =>
        {
            builder.ToTable("Subscribers");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Email).IsRequired().HasMaxLength(255);
            builder.HasIndex(s => s.Email).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}