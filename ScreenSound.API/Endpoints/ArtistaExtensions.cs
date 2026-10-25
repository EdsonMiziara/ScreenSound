using AutoMapper; 
using AutoMapper.QueryableExtensions; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScreenSound.API.Request;
using ScreenSound.API.Response;
using ScreenSound.API.Seguranca;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using ScreenSound.Shared.Data.Banco;
using ScreenSound.Shared.Data.Modelos;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ScreenSound.API.Endpoints;

public static class ArtistasExtensions
{
    private const string DefaultAvatarUrl = "https://cdn.pixabay.com/photo/2016/08/08/09/17/avatar-1577909_1280.png";

    public static void AddEndPointsArtistas(this WebApplication app)
    {
        var grupo = app.MapGroup("/Artistas")
            .RequireCors("wasm")
            .RequireAuthorization()
            .WithTags("Artistas");


        grupo.MapGet("/", async ([FromServices] DAL<Artista> dal) =>
        {
            var resposta = await dal.ListarComoQueryable().Include(a => a.Notas)
                .Select(a => new ArtistaResponse(a.Id, a.Nome, a.Bio, a.FotoPerfil) {
                    Classificacao = a.Notas
                    .Select(n => (double?)n.Nota)
                    .Average() ?? 0
                })
                .ToListAsync();

            return Results.Ok(resposta);
        });


        grupo.MapGet("/{Nome}", async (
            [FromServices] DAL<Artista> dal, string Nome) =>
        {
            var artistaResponse = await dal.RecuperarDTOAsync(
                a => a.Nome.ToUpper() == Nome.ToUpper(),
                a => new ArtistaResponse(a.Id, a.Nome, a.Bio, a.FotoPerfil));

            return artistaResponse is null ? Results.NotFound() : Results.Ok(artistaResponse);
        });

        
        grupo.MapGet("/RecuperaPor/{Id}", async (
            [FromServices] DAL<Artista> dal, int Id) =>
        {
            var artistaResponse = await dal.RecuperarDTOAsync(
                a => a.Id == Id,
                a => new ArtistaResponse(a.Id, a.Nome, a.Bio, a.FotoPerfil));

            return artistaResponse is null ? Results.NotFound() : Results.Ok(artistaResponse);
        });

        
        grupo.MapPost("/", async (
            [FromServices] IUnitOfWork uow,
            [FromServices] DAL<Artista> dal,
            [FromServices] IHostEnvironment env,
            [FromServices] ILogger<Artista> logger,
            [FromBody] ArtistaRequest artistaRequest) =>
        {
            string? imagemArtista = null;

            if (artistaRequest.fotoPerfil is not null)
            {
                var nome = artistaRequest.nome.Trim();
                var nomeSanitizado = Regex.Replace(nome.ToLower(), @"\s+", "-");
                nomeSanitizado = Regex.Replace(nomeSanitizado, @"[^a-z0-9-]", "");
                imagemArtista = $"{Guid.NewGuid()}-{nomeSanitizado}.jpeg";
            }

            var artista = new Artista(artistaRequest.nome, artistaRequest.bio)
            {
                FotoPerfil = imagemArtista is not null ? $"/FotosPerfil/{imagemArtista}" : DefaultAvatarUrl
            };

            if (!artista.EhValido)
            {
                return Results.BadRequest(artista.Erros.Sumario);
            }

            if (artistaRequest.fotoPerfil is not null && imagemArtista is not null)
            {
                var fotosPerfilPath = Path.Combine(env.ContentRootPath, "wwwroot", "FotosPerfil");
                Directory.CreateDirectory(fotosPerfilPath);

                var path = Path.Combine(fotosPerfilPath, imagemArtista);
                logger.LogInformation("Salvando foto em: {path}", path);

                using MemoryStream ms = new(Convert.FromBase64String(artistaRequest.fotoPerfil));
                using FileStream fs = new(path, FileMode.Create);
                await ms.CopyToAsync(fs);
            }

            dal.Adicionar(artista);
            await uow.CommitAsync();

            return Results.Created();
        }).RequireAuthorization(Politicas.Admin);

        grupo.MapDelete("/{Id}", async (
     [FromServices] IUnitOfWork uow,
     [FromServices] DAL<Artista> dalArtista,
     [FromServices] DAL<Musica> dalMusica,
     [FromServices] IHostEnvironment env,
     int Id) =>
        {
            var artista = await dalArtista.RecuperarComIncludeAsync(
                a => a.Id == Id,
                a => a.Musicas,
                a => a.Notas
            );

            if (artista is null) return Results.NotFound();

            if (artista.Notas is not null && artista.Notas.Any())
            {
                artista.Notas.Clear();
            }

            if (artista.Musicas is not null && artista.Musicas.Any())
            {
                return Results.BadRequest("Não é possivel excluir o artista, pois ele possui músicas atreladas a ele");
            }

            if (!string.IsNullOrEmpty(artista.FotoPerfil) && artista.FotoPerfil != DefaultAvatarUrl)
            {
                var nomeFoto = artista.FotoPerfil.Split('/').Last();
                var path = Path.Combine(env.ContentRootPath, "wwwroot", "FotosPerfil", nomeFoto);

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            dalArtista.Deletar(artista);
            await uow.CommitAsync();

            return Results.NoContent();
        }).RequireAuthorization(Politicas.Admin);

        grupo.MapPut("/", async (
            [FromServices] IUnitOfWork uow,
            [FromServices] DAL<Artista> dal,
            [FromBody] ArtistaRequestEdit artistaRequestEdit) =>
        {
            var artistaAAtualizar = await dal.RecuperarParaEdicaoAsync(a => a.Id == artistaRequestEdit.id);
            if (artistaAAtualizar is null) return Results.NotFound();

            artistaAAtualizar.Nome = artistaRequestEdit.nome;
            artistaAAtualizar.Bio = artistaRequestEdit.bio;

            if (!artistaAAtualizar.EhValido)
            {
                return Results.BadRequest(artistaAAtualizar.Erros.Sumario);
            }

            await uow.CommitAsync(); 

            return Results.Ok();
        }).RequireAuthorization(Politicas.Admin);

        grupo.MapPost("/Avaliacao/{ArtistaId}", async (
            HttpContext httpContext,
            [FromServices] IUnitOfWork uow, 
            [FromServices] DAL<Artista> dalArtista,
            [FromServices] DAL<PessoaComAcesso> dalPessoa,
            [FromBody] AvaliacaoRequest Request,
            int ArtistaId) =>
        {
            var artista = await dalArtista.RecuperarComIncludeAsync(a => a.Id == ArtistaId, a => a.Notas);
            if (artista is null) return Results.NotFound();

            var email = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (email is null) return Results.Unauthorized();

            var pessoa = await dalPessoa.RecuperarPorAsync(a => a.Email.Equals(email));
            if (pessoa is null) return Results.NotFound("Pessoa não encontrada");

            var avaliacao = artista.Notas.FirstOrDefault(a => a.PessoaId == pessoa.Id);

            if (avaliacao is null)
            {
                artista.AdicionarNota(pessoa.Id, Request.Nota);
            }
            else
            {
                avaliacao.Nota = Request.Nota;
            }

            await uow.CommitAsync(); 
            return Results.Created();
        });

        grupo.MapGet("{id}/Avaliacao", async (
            int id,
            HttpContext httpContext,
            [FromServices] DAL<Artista> dalArtista,
            [FromServices] DAL<PessoaComAcesso> dalPessoa
            ) =>
        {
            var artista = await dalArtista.RecuperarComIncludeAsync(a => a.Id == id, a => a.Notas);
            if (artista is null) return Results.NotFound();

            var email = httpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value;
            if (email is null) return Results.Unauthorized();

            var pessoa = await dalPessoa.RecuperarPorAsync(p => p.Email!.Equals(email));
            if (pessoa is null) return Results.NotFound("Pessoa não encontrada");

            var avaliacao = artista.Notas.FirstOrDefault(a => a.PessoaId == pessoa.Id);

            if (avaliacao is null) return Results.Ok(new AvaliacaoResponse(id, pessoa.Id, 0));
            else return Results.Ok(new AvaliacaoResponse(id, pessoa.Id, avaliacao.Nota));
        });
    }
}