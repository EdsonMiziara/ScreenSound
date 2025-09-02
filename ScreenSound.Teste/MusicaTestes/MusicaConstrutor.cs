using ScreenSound.Modelos;

namespace ScreenSound.Teste;

public class MusicaConstrutor
{
    [Fact] 
    public void RetornaEhValidoDeAcordoComDadosDeEntrada()
    {
        //Arrange
        var nome = "";
        bool valido = false;
        
        //Act
        var music = new Musica(nome);

        //Assert
        Assert.Equal(music.EhValido, valido);

    }

    [Fact]

    public void RetornaNaoEhValidoQuandoArtistaInvalido()
    {
        //Arrange
        var artista = new Artista("", "");
        var nome = "Musica Invalida";
        bool valido = false;
        //Act
        var music = new Musica(artista, nome);
        //Assert
        Assert.Equal(music.EhValido, valido);
        Assert.Equal(music.Erros.Count(), 1);
        Assert.Equal(music.Artista.EhValido, valido);
        Assert.Equal(music.Artista.Erros.Count(), 1);
    }

    [Theory]
    [InlineData("", null, "")]
    [InlineData("", "", "")]
    [InlineData(null, null, null)]
    [InlineData(null, "Artista", "BioArtista")]

    public void RetornaNaoEhValidoComDiferentesDadosDeEntradaInvalidos(string nome, string nomeArtista, string bio)
    {
        //Arrange
        var artista = new Artista(nomeArtista, bio);
        bool valido = false;

        //Act
        var music = new Musica(artista, nome);

        //Assert

        Assert.Equal(music.EhValido, valido);
        Assert.NotNull(music.Erros);

    }
}
