using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using ScreenSound.API.Request;
using ScreenSound.API.Response;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using System.Diagnostics.CodeAnalysis;
namespace ScreenSound.API.Endpoints;

public static class ArtistasExtensions
{
    public static void AddEndPointsArtistas(this WebApplication app)
    {
        app.MapGet("/Artistas", ([FromServices] DAL<Artista> DAL) =>
        {
            var lista = DAL.Listar();
            var resposta = EntityListToResponseList(lista);
            return Results.Ok(resposta);
        });

        app.MapGet("/Artistas/{Nome}", ([FromServices] DAL<Artista> dal, string Nome) =>
        {
            var artistaRequest = dal.RecuperarDTO(a => a.Nome.ToUpper() == Nome.ToUpper(), a => new ArtistaRequest(a.Nome, a.Bio, a.FotoPerfil));

            if (artistaRequest is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(artistaRequest);
        });

        app.MapPost("/Artistas", async ([FromServices] IHostEnvironment env,[FromServices] DAL<Artista> DAL, [FromBody] ArtistaRequest artistaRequest) =>
        {
            if (artistaRequest.fotoPerfil is not null)
            {
                var nome = artistaRequest.nome.Trim();
                var imagemArtista = DateTime.Now.ToString("ddMMyyyyhhss") + "." + nome + ".jpeg";

                var path = Path.Combine(env.ContentRootPath, "wwwroot", "FotosPerfil", imagemArtista);

                using MemoryStream ms = new MemoryStream(Convert.FromBase64String(artistaRequest.fotoPerfil!));
                using FileStream fs = new FileStream(path, FileMode.Create);
                await fs.CopyToAsync(fs);

                var artista = new Artista(artistaRequest.nome, artistaRequest.bio)
                {
                    FotoPerfil = $"/FotoPerfil/{imagemArtista}"
                };
                DAL.Adicionar(artista);
            }
            else
            {
                var artista = new Artista(artistaRequest.nome, artistaRequest.bio);
                DAL.Adicionar(artista);
            }
            return Results.Ok();
        });

        app.MapDelete("/Artistas/{Id}", ([FromServices] DAL<Artista> DAL, int Id) =>
        {
            var artista = DAL.RecuperarPor(a => a.Id == Id);
            if (artista is null)
            {
                return Results.NotFound(artista);
            }
            DAL.Deletar(artista);
            return Results.NoContent();
        });

        app.MapPut("/Artistas", ([FromServices] DAL<Artista> dal, [FromBody] ArtistaRequestEdit artistaRequestEdit) => {
            var artistaAAtualizar = dal.RecuperarPor(a => a.Id == artistaRequestEdit.id);
            if (artistaAAtualizar is null)
            {
                return Results.NotFound();
            }
            artistaAAtualizar.Nome = artistaRequestEdit.nome;
            artistaAAtualizar.Bio = artistaRequestEdit.bio;
            dal.Atualizar(artistaAAtualizar);
            return Results.Ok();
        });
    }
    private static ICollection<ArtistaResponse> EntityListToResponseList(IEnumerable<Artista> listaDeArtistas)
    {
        return listaDeArtistas.Select(a => EntityToResponse(a)).ToList();
    }

    private static ArtistaResponse EntityToResponse(Artista artista)
    {
        return new ArtistaResponse(artista.Id, artista.Nome, artista.Bio, artista.FotoPerfil);
    }
}