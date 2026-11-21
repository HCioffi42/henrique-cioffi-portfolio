using FluentAssertions;
using MeuSitePessoal.Application.Common.Models;
using MeuSitePessoal.Application.Newsletter.Commands.ConfirmSubscription;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Newsletter.Commands.ConfirmSubscription;

/// <summary>
/// Covers the second half of the newsletter double opt-in flow: the handler that turns a
/// verification link into an active subscription. The flow had no test at any level before
/// this (audit T1), despite being the step that decides whether an address ever receives mail.
/// </summary>
public class ConfirmSubscriptionCommandHandlerTests
{
    private static BlogDbContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BlogDbContext(options);
    }

    /// <summary>
    /// Adds a subscriber in the state the subscribe step leaves behind: stored, unverified,
    /// inactive, and holding a verification token.
    /// </summary>
    private static async Task<Subscriber> SeedPendingSubscriberAsync(BlogDbContext context, string email)
    {
        var subscriber = new Subscriber(email);

        context.Subscribers.Add(subscriber);
        await context.SaveChangesAsync();

        return subscriber;
    }

    [Fact]
    public async Task Handle_WithValidToken_ActivatesTheSubscription()
    {
        await using var context = BuildContext();
        var subscriber = await SeedPendingSubscriberAsync(context, "reader@test.com");
        var token = subscriber.VerificationToken!;

        var handler = new ConfirmSubscriptionCommandHandler(context);

        var result = await handler.Handle(
            new ConfirmSubscriptionCommand("reader@test.com", token),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        // Read back rather than trusting the tracked instance, so the assertion covers the save
        // and not merely the in-memory mutation the domain method performed.
        var persisted = await context.Subscribers.SingleAsync(s => s.Email == "reader@test.com");

        persisted.IsVerified.Should().BeTrue();
        persisted.IsActive.Should().BeTrue();
        persisted.VerifiedAt.Should().NotBeNull();

        // The token is single-use: it is cleared on confirmation so a leaked link cannot be
        // replayed to reactivate an address the owner later unsubscribed.
        persisted.VerificationToken.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenSubscriberDoesNotExist_ReturnsNotFound()
    {
        await using var context = BuildContext();
        var handler = new ConfirmSubscriptionCommandHandler(context);

        var result = await handler.Handle(
            new ConfirmSubscriptionCommand("stranger@test.com", "any-token"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Subscriber not found.");
        result.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WithWrongToken_LeavesTheSubscriptionPending()
    {
        await using var context = BuildContext();
        await SeedPendingSubscriberAsync(context, "reader@test.com");

        var handler = new ConfirmSubscriptionCommandHandler(context);

        var result = await handler.Handle(
            new ConfirmSubscriptionCommand("reader@test.com", "this-token-does-not-match"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid verification token.");

        var persisted = await context.Subscribers.SingleAsync(s => s.Email == "reader@test.com");

        // The substantive assertion: a rejected token must not activate the address, or the
        // double opt-in would be decorative.
        persisted.IsVerified.Should().BeFalse();
        persisted.IsActive.Should().BeFalse();
        persisted.VerifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenAlreadyVerified_SucceedsWithoutChangingTheRecord()
    {
        await using var context = BuildContext();
        var subscriber = await SeedPendingSubscriberAsync(context, "reader@test.com");
        var token = subscriber.VerificationToken!;

        var handler = new ConfirmSubscriptionCommandHandler(context);

        await handler.Handle(
            new ConfirmSubscriptionCommand("reader@test.com", token),
            CancellationToken.None);

        var verifiedAt = subscriber.VerifiedAt;

        // A confirmation link is routinely opened twice: by the mail client prefetching it and
        // then by the reader. The second visit must read as success, not as an error.
        var second = await handler.Handle(
            new ConfirmSubscriptionCommand("reader@test.com", token),
            CancellationToken.None);

        second.IsSuccess.Should().BeTrue();

        var persisted = await context.Subscribers.SingleAsync(s => s.Email == "reader@test.com");

        persisted.IsVerified.Should().BeTrue();
        persisted.IsActive.Should().BeTrue();
        persisted.VerifiedAt.Should().Be(verifiedAt);
    }

    [Fact]
    public async Task Handle_WhenTheLinkCarriesADifferentCase_StillConfirms()
    {
        await using var context = BuildContext();

        // The subscribe step stores the address lowercased, while the address travels through a
        // mail client and a query string before coming back. Both ends normalize, and this
        // pins that they agree: were they to drift, every confirmation would read as not found.
        var subscriber = await SeedPendingSubscriberAsync(context, "reader@test.com");

        var handler = new ConfirmSubscriptionCommandHandler(context);

        var result = await handler.Handle(
            new ConfirmSubscriptionCommand("Reader@Test.COM", subscriber.VerificationToken!),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var persisted = await context.Subscribers.SingleAsync(s => s.Email == "reader@test.com");

        persisted.IsVerified.Should().BeTrue();
        persisted.IsActive.Should().BeTrue();
    }
}
