using ScreenSound.Modelos;

namespace ScreenSound.Teste.ArtistaTestes;

public class ArtistaConstrutorTeste
{

    [Fact]

    public void RetornaEhValidoDeAcordoComDadosDeEntrada()
    {
        //Arrange
        var nome = "Artista Válido";
        var bio = "Artista Valido Bio";
        bool valido = true;

        //Act
        var artista = new Artista(nome, bio);

        //Assert
        Assert.Equal(artista.EhValido, valido);
        Assert.Equal(artista.Erros.Count(), 0);
    }

    [Fact]

    public void RetornaEhInvalidoComDadosNulos()
    {
        //Arrange
        string nome = null;
        string bio = null;
        bool valido = false;

        //Act
        var artista = new Artista(nome, bio);

        //Assert
        Assert.Equal(artista.EhValido, valido);
        Assert.Equal(artista.Erros.Count(), 1);
    }
}
