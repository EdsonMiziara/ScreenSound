using Xunit;
using ScreenSound.API.Requests;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ScreenSound.Teste.Integracao.Config;
using System.Net;
using ScreenSound.API.Response;
using ScreenSound.API.Request;
using ScreenSound.Modelos;
using System.Text; // Para HttpStatusCode

namespace ScreenSound.Teste.Integracao.ScreenSoundTest.ArtistaTest;

[Collection(nameof(ContextoFixture))] // Garante que a WebApplicationFactory seja compartilhada
public class Artista_Get
{
    private readonly ContextoFixture app;

    public Artista_Get(ContextoFixture app)
    {
        this.app = app;
    }

    public void Disposed()
    {
        app.Dispose();
    }

    [Fact]
     public async Task RetornaOkQuandoRecuperaArtistaPorID()
    {
        //Arrange
        var client = app.CreateClientAuthorized();

        var artistaRecuperado = app.Context.Artistas.FirstOrDefault();

        if (artistaRecuperado is null)
        {
            artistaRecuperado = new Artista("Djavan", "Djavan Caetano Viana é um cantor, compositor, arranjador, produtor musical, empresário, violonista e ex-futebolista brasileiro.");

            app.Context.Artistas.Add(artistaRecuperado);
            app.Context.SaveChanges();
        }
        //Act

        var reponse = await client.GetFromJsonAsync<ArtistaResponse>($"/Artistas/RecuperaPor/{artistaRecuperado.Id}");

        //Assert

        Assert.NotNull(reponse);
        Assert.Equal(artistaRecuperado.Nome, reponse.Nome);
        Assert.Equal(artistaRecuperado.Bio, reponse.Bio);
    }

    [Fact]
    public async Task RetornaNotFoundQuandoRecuperaArtistaPorIDErrado()
    {
        //Arrange
        var client = app.CreateClientAuthorized();

        //Act
        var response = await client.GetAsync("/Artistas/RecuperaPor/4");

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
    }
    [Fact]

    public async Task RetornaUnAuthorizedQuandoRecuperaArtistaSemAuthorização()
    {
        //Arrange
        var client = app.CreateClientUnauthorized();

        //Act
        var response = await client.GetAsync("/Artistas/");

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

    }
}