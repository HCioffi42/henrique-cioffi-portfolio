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
        var artigo = new Article(tituloEsperado, conteudoEsperado, resumoEsperado, tagsEsperadas, ArticleCategory.Technology);

        // Assert
        Assert.NotNull(artigo);
        Assert.Equal(tituloEsperado, artigo.Title);
        Assert.Equal(resumoEsperado, artigo.Summary);
        Assert.Equal(tagsEsperadas, artigo.Tags);
        Assert.Equal(ArticleCategory.Technology, artigo.Category);
    }

    [Fact]
    public void CriarArtigo_ComCategoriaEspecifica_DeveAtribuirCorretamente()
    {
        // Arrange
        var categoria = ArticleCategory.Tutorial;

        // Act
        var artigo = new Article("Title", "Content", "Summary", new List<string>(), categoria);

        // Assert
        Assert.Equal(categoria, artigo.Category);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CriarArtigo_ComTituloInvalido_DeveLancarExcecao(string tituloInvalido)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Article(tituloInvalido, "Conteúdo qualquer", "Summary", new List<string>(), ArticleCategory.Technology));
    }
}