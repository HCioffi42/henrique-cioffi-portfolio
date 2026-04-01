using MeuSitePessoal.Application.Articles.Commands.CreateArticle;
using Moq;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Application.Articles.Commands;
using Xunit;
using Microsoft.Extensions.Caching.Memory;

namespace MeuSitePessoal.Tests.Unit.Application.Handlers;

public class CreateArticleHandlerTests
{
    private readonly Mock<IArticleRepository> _repositoryMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly CreateArticleHandler _handler;

    public CreateArticleHandlerTests()
    {
        _repositoryMock = new Mock<IArticleRepository>();
        _cacheMock = new Mock<IMemoryCache>();
        _handler = new CreateArticleHandler(_repositoryMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveChamarRepositorioERetornarId()
    {
        // Arrange
        var command = new CreateArticleCommand(
            "Título do Article",
            "Conteúdo completo do article.",
            "Summary do article.",
            ArticleCategory.Technology,
            new List<string> { "tag1", "tag2" }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Article>(a => 
            a.Title == command.Title && 
            a.Content == command.Content &&
            a.Summary == command.Summary &&
            a.Category == command.Category &&
            a.Tags.SequenceEqual(command.Tags)
        )), Times.Once);
    }
}
