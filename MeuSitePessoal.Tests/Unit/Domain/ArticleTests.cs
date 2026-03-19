using MeuSitePessoal.Domain;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Domain;

public class ArticleTests
{
    [Fact]
    public void CriarArtigo_ComDadosValidos_DeveRetornarSucesso()
    {
        // Arrange
        var tituloEsperado = "TDD na Prática";
        var conteudoEsperado = "Conteúdo sobre testes unitários.";
        var resumoEsperado = "Um guia prático sobre TDD.";
        var tagsEsperadas = new List<string> { "tdd", "dotnet" };

        // Act
        var artigo = new Article(tituloEsperado, conteudoEsperado, resumoEsperado, tagsEsperadas);

        // Assert
        Assert.NotNull(artigo);
        Assert.Equal(tituloEsperado, artigo.Title);
        Assert.Equal(resumoEsperado, artigo.Summary);
        Assert.Equal(tagsEsperadas, artigo.Tags);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CriarArtigo_ComTituloInvalido_DeveLancarExcecao(string tituloInvalido)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Article(tituloInvalido, "Conteúdo qualquer", "Summary", new List<string>()));
    }
}