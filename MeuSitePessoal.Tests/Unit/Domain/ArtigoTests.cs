using MeuSitePessoal.Domain;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Domain;

public class ArtigoTests
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
        var artigo = new Artigo(tituloEsperado, conteudoEsperado, resumoEsperado, tagsEsperadas);

        // Assert
        Assert.NotNull(artigo);
        Assert.Equal(tituloEsperado, artigo.Titulo);
        Assert.Equal(resumoEsperado, artigo.Resumo);
        Assert.Equal(tagsEsperadas, artigo.Tags);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CriarArtigo_ComTituloInvalido_DeveLancarExcecao(string tituloInvalido)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Artigo(tituloInvalido, "Conteúdo qualquer", "Resumo", new List<string>()));
    }
}