using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using ScreenSound.API.Request;
using ScreenSound.API.Response;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using ScreenSound.Shared.Data.Modelos;
using ScreenSound.Shared.Modelos.Avaliacao;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
namespace ScreenSound.API.Endpoints;

public static class ArtistasExtensions
{
    public static void AddEndPointsArtistas(this WebApplication app)
    {
        var grupo = app.MapGroup("/Artistas")
            .RequireCors("wasm")
            .RequireAuthorization()
            .WithTags("Artistas");

        grupo.MapGet("/", ([FromServices] DAL<Artista> DAL) =>
        {
            var lista = DAL.Listar();
            var resposta = EntityListToResponseList(lista);
            return Results.Ok(resposta);
        });

        grupo.MapGet("/{Nome}", ([FromServices] DAL<Artista> dal, string Nome) =>
        {
            var artistaResponse = dal.RecuperarDTO(a => a.Nome.ToUpper() == Nome.ToUpper(), a => new ArtistaResponse(a.Id, a.Nome, a.Bio, a.FotoPerfil));
            return artistaResponse is null ? Results.NotFound() : Results.Ok(artistaResponse);
        });

        grupo.MapGet("/RecuperaPor/{Id}", ([FromServices] DAL<Artista> dal, int Id) =>
        {
            var artistaResponse = dal.RecuperarDTO(a => a.Id == Id, a => new ArtistaResponse(a.Id, a.Nome, a.Bio, a.FotoPerfil));
            return artistaResponse is null ? Results.NotFound() : Results.Ok(artistaResponse);
        });

        grupo.MapPost("/", async ([FromServices] IHostEnvironment env, [FromServices] DAL<Artista> DAL, [FromBody] ArtistaRequest artistaRequest) =>
        {
            if (artistaRequest.fotoPerfil is not null)
            {
                var nome = artistaRequest.nome.Trim();
                var imagemArtista = DateTime.Now.ToString("ddMMyyyyhhss") + "." + nome + ".jpeg";
                var path = Path.Combine(env.ContentRootPath, "wwwroot", "FotosPerfil", imagemArtista);

                using MemoryStream ms = new(Convert.FromBase64String(artistaRequest.fotoPerfil!));
                using FileStream fs = new(path, FileMode.Create);
                await ms.CopyToAsync(fs);

                var artista = new Artista(artistaRequest.nome, artistaRequest.bio)
                {
                    FotoPerfil = $"/FotoPerfil/{imagemArtista}"
                };
                if (!artista.EhValido)
                {
                    return Results.BadRequest(artista.Erros.Sumario);
                }
                DAL.Adicionar(artista);
                return Results.Created();
            }
            else
            {
                var artista = new Artista(artistaRequest.nome, artistaRequest.bio);
                if (!artista.EhValido)
                {
                    return Results.BadRequest(artista.Erros.Sumario);

                }
                DAL.Adicionar(artista);
                return Results.Created();
            }
        });

        grupo.MapDelete("/{Id}", ([FromServices] DAL<Artista> DAL, int Id) =>
        {
            var artista = DAL.RecuperarPor(a => a.Id == Id);
            if (artista is null) return Results.NotFound();
            if (artista.FotoPerfil != null && artista.FotoPerfil != "https://cdn.pixabay.com/photo/2016/08/08/09/17/avatar-1577909_1280.png")
            {
                var nomeFoto = artista.FotoPerfil.Split('/').Last();
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FotosPerfil", nomeFoto);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            DAL.Deletar(artista);
            return Results.NoContent();
        });

        grupo.MapPut("/", ([FromServices] DAL<Artista> dal, [FromBody] ArtistaRequestEdit artistaRequestEdit) =>
        {
            var artistaAAtualizar = dal.RecuperarPor(a => a.Id == artistaRequestEdit.id);
            if (artistaAAtualizar is null) return Results.NotFound();

            artistaAAtualizar.Nome = artistaRequestEdit.nome;
            artistaAAtualizar.Bio = artistaRequestEdit.bio;
            if (!artistaAAtualizar.EhValido)
            {
                return Results.BadRequest(artistaAAtualizar.Erros.Sumario);
            }

            dal.Atualizar(artistaAAtualizar);
            return Results.Ok();
        });

        grupo.MapPost("/Avaliacao/{ArtistaId}",(HttpContext context,            
            [FromBody] AvaliacaoRequest Request,
            [FromServices] DAL<Artista> dalArtista,
            [FromServices] DAL<PessoaComAcesso> dalPessoa,
            int ArtistaId ) =>                
        {
            var artista = dalArtista.RecuperarPor(a => a.Id == ArtistaId);
            if (artista is null)
            {
                return Results.NotFound();
            }

            var email = context.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                ?? throw new InvalidOperationException("Pessoa não está conectada");

            var pessoa = dalPessoa.RecuperarPor(a => a.Email.Equals(email)) 
                ?? throw new InvalidOperationException("Pessoa não encontrada");

            var avaliacao = artista.Notas
                .FirstOrDefault(a => a.PessoaId == pessoa.Id);

            if (avaliacao is null) 
            {
                artista.AdicionarNota(pessoa.Id, Request.Nota);
            }
            else
            {
                avaliacao.Nota = Request.Nota;
            }
            dalArtista.Atualizar(artista);
            return Results.Created();
        });
        grupo.MapGet("{id}/Avaliacao", (
            int id,
            HttpContext context,
            [FromServices] DAL<Artista> dalArtista,
            [FromServices] DAL<PessoaComAcesso> dalPessoa
         ) =>
        {
            var artista = dalArtista.RecuperarPor(a => a.Id == id);
            if (artista is null) return Results.NotFound();
            var email = context.User.Claims
                .FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value
                ?? throw new InvalidOperationException("Não foi encontrado o email da pessoa logada");

            var pessoa = dalPessoa.RecuperarPor(p => p.Email!.Equals(email))
                ?? throw new InvalidOperationException("Não foi encontrado o email da pessoa logada");

            var avaliacao = artista
                .Notas
                .FirstOrDefault(a => a.PessoaId == pessoa.Id);

            if (avaliacao is null) return Results.Ok(new AvaliacaoResponse(id, pessoa.Id, 0));
            else return Results.Ok(new AvaliacaoResponse(id, pessoa.Id, avaliacao.Nota));
        });
    }

   

    private static ICollection<ArtistaResponse> EntityListToResponseList(IEnumerable<Artista> listaDeArtistas)
    {
        return listaDeArtistas.Select(a => EntityToResponse(a)).ToList();
    }

    private static ArtistaResponse EntityToResponse(Artista artista)
    {
        return new ArtistaResponse(artista.Id, artista.Nome, artista.Bio, artista.FotoPerfil)
        {
            Classificacao = artista.Notas.Select(a => a.Nota).DefaultIfEmpty(0).Average()
        };
    }
}