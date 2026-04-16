using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Application.Newsletter.Commands.Subscribe;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Newsletter;

public class SubscribeToNewsletterCommandHandlerTests
{
    private readonly Mock<IEmailSender> _emailSenderMock = new();
    private readonly Mock<IEmailTemplateService> _templateServiceMock = new();
    private readonly Mock<IConfiguration> _configMock = new();


    private BlogDbContext GetMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new BlogDbContext(options);
    }

    [Fact]
    public async Task Handle_Should_CreateUnverifiedSubscriber_When_EmailIsNew()
    {
        // Arrange
        var context = GetMemoryContext();
        var handler = new SubscribeToNewsletterCommandHandler(context, _emailSenderMock.Object, _templateServiceMock.Object, _configMock.Object);
        var command = new SubscribeToNewsletterCommand("new@test.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        var subscriber = await context.Subscribers.FirstAsync();
        Assert.False(subscriber.IsVerified);
        Assert.False(subscriber.IsActive);
        Assert.NotNull(subscriber.VerificationToken);
        
        _emailSenderMock.Verify(x => x.SendEmailAsync(
            "new@test.com", 
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_EmailAlreadyVerifiedAndActive()
    {
        // Arrange
        var context = GetMemoryContext();
        var existing = new Subscriber("verified@test.com");
        existing.IsVerified = true;
        existing.IsActive = true;
        
        context.Subscribers.Add(existing);
        await context.SaveChangesAsync();

        var handler = new SubscribeToNewsletterCommandHandler(context, _emailSenderMock.Object, _templateServiceMock.Object, _configMock.Object);
        var command = new SubscribeToNewsletterCommand("verified@test.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _emailSenderMock.Verify(x => x.SendEmailAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), Times.Never);
        
    }

    [Fact]
    public async Task Handle_Should_ResendToken_When_EmailExistsButNotVerified()
    {
        // Arrange
        var context = GetMemoryContext();
        var existing = new Subscriber("unverified@test.com");
        existing.IsVerified = false;
        existing.IsActive = false;
        
        existing.UpdateVerificationToken("old-token");
    
        context.Subscribers.Add(existing);
        await context.SaveChangesAsync();

        var handler = new SubscribeToNewsletterCommandHandler(context, _emailSenderMock.Object, _templateServiceMock.Object, _configMock.Object);
        var command = new SubscribeToNewsletterCommand("unverified@test.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        var updated = await context.Subscribers.FirstAsync();
        Assert.NotEqual("old-token", updated.VerificationToken);
        
        _emailSenderMock.Verify(x => x.SendEmailAsync(
            "unverified@test.com", 
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

