using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeuSitePessoal.Infrastructure.Data;

public class BlogDbContext : IdentityDbContext, IBlogDbContext
{
    // Defines the context that inherits from IdentityDbContext to manage communication with PostgreSQL and Identity tables.
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }

    // Maps the Article entity to a table called Articles in the database.
    public DbSet<Article> Articles { get; set; }

    // Maps the Subscriber entity to a table called Subscribers in the database.
    public DbSet<Subscriber> Subscribers { get; set; }

    // Maps the Comment entity to a table called Comments in the database.
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configures the Article entity to ensure that business rules are reflected in the database schema.
        modelBuilder.Entity<Article>(builder =>
        {
            builder.ToTable("Articles");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
            builder.Property(a => a.Content).IsRequired();
            builder.Property(a => a.Summary).IsRequired().HasMaxLength(500);
            builder.Property(a => a.Tags).IsRequired();
            
            // Stores the enum as a string in the database to prevent data corruption if the enum order changes in code.
            builder.Property(a => a.Category)
                   .IsRequired()
                   .HasConversion<string>();

            builder.Property(a => a.CreatedAt).IsRequired();

            // Configures the one-to-many relationship between Article and Comments.
            builder.HasMany(a => a.Comments)
                   .WithOne(c => c.Article)
                   .HasForeignKey(c => c.ArticleId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // Configures the Subscriber entity with unique index for the email.
        modelBuilder.Entity<Subscriber>(builder =>
        {
            builder.ToTable("Subscribers");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Email).IsRequired().HasMaxLength(255);
            builder.HasIndex(s => s.Email).IsUnique();
        });

        // Configures the Comment entity with self-referencing relationship for nesting.
        modelBuilder.Entity<Comment>(builder =>
        {
            builder.ToTable("Comments");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Content).IsRequired().HasMaxLength(2000);
            builder.Property(c => c.AuthorName).IsRequired().HasMaxLength(100);
            builder.Property(c => c.CreatedAt).IsRequired();
            builder.Property(c => c.UserId).HasMaxLength(450); // Standard Identity User Id length

            // Self-referencing relationship for nested replies.
            builder.HasOne(c => c.ParentComment)
                   .WithMany(c => c.Replies)
                   .HasForeignKey(c => c.ParentCommentId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}