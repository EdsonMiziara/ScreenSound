using Microsoft.AspNetCore.Mvc;
using ScreenSound.API.Request;
using ScreenSound.API.Requests;
using ScreenSound.Banco;
using ScreenSound.Modelos;

namespace ScreenSound.API.Endpoints;

public static class GeneroExtensions
{
    public static void AddEndpointsGenero(this WebApplication app)
    {
        app.MapGet("/Generos", ([FromServices] DAL<Genero> dal) =>
        {
            return Results.Ok(dal.Listar().Select(g => new GeneroRequest(g.Nome, g.Descricao)));
        });

        app.MapGet("/Generos/{nome}", ([FromServices] DAL<Genero> dal, string nome) => 
        {
            #nullable disable
            var GeneroRequest = dal.RecuperarDTO(a => a.Nome.ToUpper().Equals(nome.ToUpper()), a => new GeneroRequest(a.Nome, a.Descricao));
            if (GeneroRequest is null) 
            {
                return Results.NotFound();
            }
            return Results.Ok(GeneroRequest);
        });

        app.MapPost("/Generos", ([FromServices] DAL<Genero> dal, [FromBody] GeneroRequest generoRequest) =>
        {
            var genero = new Genero(generoRequest.Nome, generoRequest.DescricaoGenero);
            dal.Adicionar(genero);
            return Results.Ok(genero);

        });

        app.MapDelete("/Generos/{id}", ([FromServices] DAL<Genero> dal, int id) => 
        {
            var musica = dal.RecuperarPor(a => a.Id == id);
            if (musica is null)
            {
                return Results.NotFound();
            }
            dal.Deletar(musica);
            return Results.NoContent();
        });

        app.MapPut("/Generos", ([FromServices] DAL<Genero> dal, [FromBody] GeneroRequestEdit generoRequestEdit) =>
        {
            var generoAAtualizar = dal.RecuperarPor(a => a.Id == generoRequestEdit.IdEdit);
            if (generoAAtualizar is null)
            {
                return Results.NotFound();
            }
            generoAAtualizar.Nome = generoRequestEdit.NomeEdit;
            generoAAtualizar.Descricao = generoRequestEdit.DescricaoEdit;

            dal.Atualizar(generoAAtualizar);
            return Results.NoContent();
        });
    }
}
