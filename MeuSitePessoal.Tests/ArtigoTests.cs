using MeuSitePessoal.Domain;
using Xunit;

namespace MeuSitePessoal.Tests;

public class ArtigoTests
{
    [Fact]
    public void CriarArtigo_ComDadosValidos_DeveRetornarSucesso()
    {
        // Arrange
        var tituloEsperado = "TDD na Prática";
        var conteudoEsperado = "Conteúdo sobre testes unitários.";

        // Act
        var artigo = new Artigo(tituloEsperado, conteudoEsperado);

        // Assert
        Assert.NotNull(artigo);
        Assert.Equal(tituloEsperado, artigo.Titulo);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CriarArtigo_ComTituloInvalido_DeveLancarExcecao(string tituloInvalido)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Artigo(tituloInvalido, "Conteúdo qualquer"));
    }
}
// A classe de teste valida tanto o cenário de sucesso na criação de um artigo quanto o lançamento de exceção para títulos inválidos usando Theory do xUnit.