using FluentAssertions;
using MeuSitePessoal.Application.Newsletter.Commands.Unsubscribe;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Newsletter.Commands.Unsubscribe;

public class UnsubscribeHandlerTests
{
    private readonly Mock<ISubscriberRepository> _subscriberRepositoryMock;
    private readonly Mock<ILogger<UnsubscribeHandler>> _loggerMock;
    private readonly UnsubscribeHandler _handler;

    public UnsubscribeHandlerTests()
    {
        _subscriberRepositoryMock = new Mock<ISubscriberRepository>();
        _loggerMock = new Mock<ILogger<UnsubscribeHandler>>();
        _handler = new UnsubscribeHandler(_subscriberRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidToken_ShouldUnsubscribeSuccessfully()
    {
        // Arrange
        var email = "test@example.com";
        var subscriber = new Subscriber(email);
        var validToken = subscriber.UnsubscribeToken!;

        _subscriberRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(subscriber);

        var command = new UnsubscribeCommand(email, validToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        subscriber.IsActive.Should().BeFalse();
        _subscriberRepositoryMock.Verify(x => x.UpdateAsync(subscriber), Times.Once);
    }

    [Fact]
    public async Task Handle_SubscriberNotFound_ShouldReturnFailure()
    {
        // Arrange
        var email = "notfound@example.com";
        _subscriberRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((Subscriber?)null);

        var command = new UnsubscribeCommand(email, "any-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Subscriber not found.");
    }

    [Fact]
    public async Task Handle_InvalidToken_ShouldReturnFailure()
    {
        // Arrange
        var email = "test@example.com";
        var subscriber = new Subscriber(email);
        var wrongToken = "this-token-does-not-match";

        _subscriberRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(subscriber);

        var command = new UnsubscribeCommand(email, wrongToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid unsubscribe token.");
        subscriber.IsActive.Should().BeTrue(); // Should remain active
    }
}
