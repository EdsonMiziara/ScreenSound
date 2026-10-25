using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using ScreenSound.API.Request;
using ScreenSound.API.Requests;
using ScreenSound.API.Seguranca;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using ScreenSound.Shared.Data.Banco;

namespace ScreenSound.API.Endpoints;

public static class GeneroExtensions
{
    public static void AddEndpointsGenero(this WebApplication app)
    {
        var grupo = app.MapGroup("/Generos")
            .RequireCors("wasm")
            .RequireAuthorization()
            .WithTags("Generos");

        grupo.MapGet("/", async ([FromServices] DAL<Genero> dal) =>
        {
            var lista = await dal.ListarComoQueryable()
                .Select(g => new GeneroRequest(g.Nome!, g.Descricao))
                .ToListAsync();

            return Results.Ok(lista);
        });

        grupo.MapGet("/{nome}", async ([FromServices] DAL<Genero> dal, string nome) =>
        {
            var generoRequest = await dal.RecuperarDTOAsync(
                a => a.Nome!.ToUpper().Equals(nome.ToUpper()),
                a => new GeneroRequest(a.Nome!, a.Descricao)
            );

            return generoRequest is null ? Results.NotFound() : Results.Ok(generoRequest);
        });

        grupo.MapPost("/", async (
            [FromServices] IUnitOfWork uow, 
            [FromServices] DAL<Genero> dal,
            [FromBody] GeneroRequest generoRequest) =>
        {
            var genero = new Genero(generoRequest.Nome, generoRequest.DescricaoGenero!);
            if (!genero.EhValido)
            {
                return Results.BadRequest(genero.Erros.Sumario);
            }

            dal.Adicionar(genero);
            await uow.CommitAsync(); 

            return Results.Created(); 
        }).RequireAuthorization(Politicas.Admin);

        grupo.MapPut("/", async (
            [FromServices] IUnitOfWork uow,
            [FromServices] DAL<Genero> dal,
            [FromBody] GeneroRequestEdit generoRequestEdit) =>
        {
            var genero = await dal.RecuperarParaEdicaoAsync(a => a.Id == generoRequestEdit.IdEdit);
            if (genero is null) return Results.NotFound();

            genero.Nome = generoRequestEdit.NomeEdit;
            genero.Descricao = generoRequestEdit.DescricaoEdit;

            if (!genero.EhValido)
            {
                return Results.BadRequest(genero.Erros.Sumario);
            }

            await uow.CommitAsync();

            return Results.Ok();
        }).RequireAuthorization(Politicas.Admin);

        grupo.MapDelete("/{id}", async (
            [FromServices] IUnitOfWork uow, 
            [FromServices] DAL<Genero> dal,
            int id) =>
        {
            var genero = await dal.RecuperarPorAsync(a => a.Id == id);
            if (genero is null) return Results.NotFound();

            dal.Deletar(genero);
            await uow.CommitAsync(); 

            return Results.NoContent();
        }).RequireAuthorization(Politicas.Admin);
    }
}