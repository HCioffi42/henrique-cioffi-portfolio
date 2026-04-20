using MeuSitePessoal.Application.Articles.Commands.CreateArticle;
using Moq;
using MediatR;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Application.Articles.Commands;
using MeuSitePessoal.Domain.Entities;
using Xunit;
using Microsoft.Extensions.Caching.Memory;

namespace MeuSitePessoal.Tests.Unit.Application.Handlers;

public class CreateArticleHandlerTests
{
    private readonly Mock<IArticleRepository> _repositoryMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CreateArticleHandler _handler;

    public CreateArticleHandlerTests()
    {
        _repositoryMock = new Mock<IArticleRepository>();
        _cacheMock = new Mock<IMemoryCache>();
        _mediatorMock = new Mock<IMediator>();
        _handler = new CreateArticleHandler(_repositoryMock.Object, _cacheMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveChamarRepositorioERetornarId()
    {
        // Arrange
        var command = new CreateArticleCommand(
            "English Title",
            "Portuguese Title",
            "English Content",
            "Portuguese Content",
            "English Summary",
            "Portuguese Summary",
            ArticleCategory.Technology,
            new List<string> { "tag1", "tag2" }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<MeuSitePessoal.Domain.Events.ArticlePublishedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Article>(a => 
            a.TitleEn == command.TitleEn && 
            a.TitlePt == command.TitlePt &&
            a.ContentEn == command.ContentEn &&
            a.ContentPt == command.ContentPt &&
            a.SummaryEn == command.SummaryEn &&
            a.SummaryPt == command.SummaryPt &&
            a.Category == command.Category &&
            a.Tags.SequenceEqual(command.Tags)
        )), Times.Once);
    }
}
