using ScreenSound.Modelos;
using ScreenSound.Teste.Integracao.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ScreenSound.API.Request;

namespace ScreenSound.Teste.Integracao.ScreenSoundTest.ArtistaTest;

[Collection(nameof(ContextoFixture))]
public class Artista_Put 
{
    public readonly ContextoFixture app;

    public Artista_Put(ContextoFixture app)
    {
        this.app = app;
    }

    [Fact]
    public async Task Atualiza_Artista()
    {
        //Arrange
        var artistaExistente = app.Context.Artistas.FirstOrDefault();
        if (artistaExistente is null)
        {
            artistaExistente = new Artista("João Arthur", "Artista de pagode carioca.");
            app.Context.Add(artistaExistente);
            app.Context.SaveChanges();
        }

        using var client = app.CreateClient();

        artistaExistente.Nome = "João Arthur. [Atualizado]";
        artistaExistente.Bio = "Artista de pagode carioca. [Atualizada]";
        //Act
        var response = await client.PutAsJsonAsync($"/Artistas", new ArtistaRequestEdit(artistaExistente.Id, artistaExistente.Nome, artistaExistente.Bio, "fotoPerfil"));

        //Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    }
}
