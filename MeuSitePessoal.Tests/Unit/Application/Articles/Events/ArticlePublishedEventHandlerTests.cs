using MeuSitePessoal.Application.Articles.Events;
using MeuSitePessoal.Application.Common.Interfaces;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;
using MeuSitePessoal.Domain.Events;
using MeuSitePessoal.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Articles.Events;

public class ArticlePublishedEventHandlerTests
{
    private readonly Mock<ISubscriberRepository> _subscriberRepositoryMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly Mock<IEmailTemplateService> _templateServiceMock;
    private readonly Mock<ILogger<ArticlePublishedEventHandler>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ArticlePublishedEventHandler _handler;

    public ArticlePublishedEventHandlerTests()
    {
        _subscriberRepositoryMock = new Mock<ISubscriberRepository>();
        _emailSenderMock = new Mock<IEmailSender>();
        _templateServiceMock = new Mock<IEmailTemplateService>();
        _loggerMock = new Mock<ILogger<ArticlePublishedEventHandler>>();
        _configurationMock = new Mock<IConfiguration>();

        _handler = new ArticlePublishedEventHandler(
            _subscriberRepositoryMock.Object,
            _emailSenderMock.Object,
            _templateServiceMock.Object,
            _loggerMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_WithActiveSubscribers_ShouldSendEmails()
    {
        // Arrange
        var article = new Article("Title", "Título", "Content", "Conteúdo", "Summary", "Resumo", new List<string>(), ArticleCategory.Technology);
        var notification = new ArticlePublishedEvent(article);
        
        // Creates a list of subscribers using the domain constructor to respect encapsulation.
        // The UnsubscribeToken is generated internally by the constructor.
        var sub1 = new Subscriber("s1@test.com");
        sub1.IsActive = true;
        sub1.IsVerified = true;

        var sub2 = new Subscriber("s2@test.com");
        sub2.IsActive = true;
        sub2.IsVerified = true;
        
        var subscribers = new List<Subscriber> { sub1, sub2 };

        // Mocks the repository to return the prepared active subscribers.
        _subscriberRepositoryMock.Setup(x => x.GetActiveSubscribersAsync())
            .ReturnsAsync(subscribers);

        // Mocks the template service to return a dummy HTML body for the notification.
        _templateServiceMock.Setup(x => x.RenderTemplateAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync("<html>body</html>");

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _emailSenderMock.Verify(x => x.SendEmailAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WithNoSubscribers_ShouldNotSendEmails()
    {
        // Arrange
        var article = new Article("Title", "Título", "Content", "Conteúdo", "Summary", "Resumo", new List<string>(), ArticleCategory.Technology);
        var notification = new ArticlePublishedEvent(article);
        
        _subscriberRepositoryMock.Setup(x => x.GetActiveSubscribersAsync())
            .ReturnsAsync(new List<Subscriber>());

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _emailSenderMock.Verify(x => x.SendEmailAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }
}