using Moq;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Interfaces;
using MeuSitePessoal.Application.Commands;
using MeuSitePessoal.Application.Handlers;
using Xunit;

namespace MeuSitePessoal.Tests;

public class CreateArtigoHandlerTests
{
    private readonly Mock<IArtigoRepository> _repositoryMock;
    private readonly CreateArtigoHandler _handler;

    public CreateArtigoHandlerTests()
    {
        _repositoryMock = new Mock<IArtigoRepository>();
        _handler = new CreateArtigoHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveChamarRepositorioERetornarId()
    {
        // Arrange
        var command = new CreateArtigoCommand(
            "Título do Artigo",
            "Conteúdo completo do artigo.",
            "Resumo do artigo.",
            new List<string> { "tag1", "tag2" }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _repositoryMock.Verify(r => r.AdicionarAsync(It.Is<Artigo>(a => 
            a.Titulo == command.Titulo && 
            a.Conteudo == command.Conteudo &&
            a.Resumo == command.Resumo &&
            a.Tags == command.Tags
        )), Times.Once);
    }
}
