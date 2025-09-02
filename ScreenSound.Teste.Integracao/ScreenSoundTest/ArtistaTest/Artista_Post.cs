using ScreenSound.API.Request;
using ScreenSound.Modelos;
using ScreenSound.Teste.Integracao.Config;
using System.Net;
using System.Net.Http.Json;

namespace ScreenSound.Teste.Integracao.ScreenSoundTest.ArtistaTest;

[Collection(nameof(ContextoFixture))]
public class Artista_Post 
{
    private readonly ContextoFixture app;

    public Artista_Post(ContextoFixture app)
    {
        this.app = app; 
    }

    [Fact]

    public async Task RetornaCreatedQuandoCadastraArtistaValido()
    {
        //Arrange
        using var client = app.CreateClient();

        var artista = new ArtistaRequest("Artista", "bio");

        //Act

        var response = await client.PostAsJsonAsync("/Artistas", artista);

        //Assert

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    }

    [Fact]

    public async Task RetornaBadRequestQuandoCadastraArtistaNaoValido()
    {
        //Arrange
        using var client = app.CreateClient();
        var artista = new ArtistaRequest("", "bio");
        //Act
        var response = await client.PostAsJsonAsync("/Artistas", artista);
        //Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
    }

}

