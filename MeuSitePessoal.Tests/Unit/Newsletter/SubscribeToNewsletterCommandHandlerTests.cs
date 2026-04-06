using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Application.Newsletter.Commands.Subscribe;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Newsletter;

public class SubscribeToNewsletterCommandHandlerTests
{
    private BlogDbContext GetMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new BlogDbContext(options);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_NewEmailProvided()
    {
        // Arrange
        var context = GetMemoryContext();
        var handler = new SubscribeToNewsletterCommandHandler(context);
        var command = new SubscribeToNewsletterCommand("newuser@test.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        var savedSubscriber = await context.Subscribers.FirstOrDefaultAsync();
        Assert.NotNull(savedSubscriber);
        Assert.Equal("newuser@test.com", savedSubscriber.Email);
        Assert.True(savedSubscriber.IsActive);
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_EmailAlreadyActive()
    {
        // Arrange
        var context = GetMemoryContext();
        var existingSubscriber = new Subscriber
        {
            Id = Guid.NewGuid(),
            Email = "existing@test.com",
            IsActive = true,
            SubscribedAt = DateTime.UtcNow
        };
        context.Subscribers.Add(existingSubscriber);
        await context.SaveChangesAsync();

        var handler = new SubscribeToNewsletterCommandHandler(context);
        var command = new SubscribeToNewsletterCommand("existing@test.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Type);
        Assert.Equal("This email is already subscribed.", result.Error);
    }

    [Fact]
    public async Task Handle_Should_Reactivate_When_EmailExistsButInactive()
    {
        // Arrange
        var context = GetMemoryContext();
        var existingSubscriber = new Subscriber
        {
            Id = Guid.NewGuid(),
            Email = "inactive@test.com",
            IsActive = false,
            SubscribedAt = DateTime.UtcNow.AddDays(-10)
        };
        context.Subscribers.Add(existingSubscriber);
        await context.SaveChangesAsync();

        var handler = new SubscribeToNewsletterCommandHandler(context);
        var command = new SubscribeToNewsletterCommand("inactive@test.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        var updatedSubscriber = await context.Subscribers.FirstAsync(x => x.Email == "inactive@test.com");
        Assert.True(updatedSubscriber.IsActive);
        
        // Ensure only one subscriber exists in DB
        Assert.Equal(1, await context.Subscribers.CountAsync());
    }
}
