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
        var grupo = app.MapGroup("/Generos").RequireCors("wasm");

        grupo.MapGet("/", ([FromServices] DAL<Genero> dal) =>
        {
            return Results.Ok(dal.Listar().Select(g => new GeneroRequest(g.Nome!, g.Descricao)));
        });

        grupo.MapGet("/{nome}", ([FromServices] DAL<Genero> dal, string nome) =>
        {
            var generoRequest = dal.RecuperarDTO(
                a => a.Nome!.ToUpper().Equals(nome.ToUpper()),
                a => new GeneroRequest(a.Nome!, a.Descricao)
            );
            return generoRequest is null ? Results.NotFound() : Results.Ok(generoRequest);
        });

        grupo.MapPost("/", ([FromServices] DAL<Genero> dal, [FromBody] GeneroRequest generoRequest) =>
        {
            var genero = new Genero(generoRequest.Nome, generoRequest.DescricaoGenero!);
            dal.Adicionar(genero);
            return Results.Ok(genero);
        });

        grupo.MapPut("/", ([FromServices] DAL<Genero> dal, [FromBody] GeneroRequestEdit generoRequestEdit) =>
        {
            var genero = dal.RecuperarPor(a => a.Id == generoRequestEdit.IdEdit);
            if (genero is null) return Results.NotFound();

            genero.Nome = generoRequestEdit.NomeEdit;
            genero.Descricao = generoRequestEdit.DescricaoEdit;
            dal.Atualizar(genero);
            return Results.NoContent();
        });

        grupo.MapDelete("/{id}", ([FromServices] DAL<Genero> dal, int id) =>
        {
            var genero = dal.RecuperarPor(a => a.Id == id);
            return genero is null ? Results.NotFound() : Results.NoContent();
        });
    }
}
