using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using ScreenSound.API.Request;
using ScreenSound.API.Requests;
using ScreenSound.API.Response;
using ScreenSound.Shared.Data.Banco;
using ScreenSound.Models;
namespace ScreenSound.API.Endpoints;

public static class ArtistasExtensions
{
    public static void AddEndPointsArtistas(this WebApplication app)
    {
        app.MapGet("/Artistas", ([FromServices] DAL<Artista> DAL) =>
        {
            return Results.Ok(DAL.Listar());
        });

        app.MapGet("/Artistas/{Nome}", ([FromServices] DAL<Artista> dal, string Nome) =>
        {
        var artistaRequest = dal.RecuperarDTO(a => a.Nome.ToUpperInvariant() == Nome.ToUpperInvariant(), a => new ArtistaRequest(a.Nome, a.Bio));

            if (artistaRequest is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(artistaRequest);
        });

        app.MapPost("/Artistas", ([FromServices] DAL<Artista> DAL, [FromBody] ArtistaRequest artistaRequest) =>
        {
            var artista = new Artista(artistaRequest.nome, artistaRequest.bio);
            DAL.Adicionar(artista);
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

