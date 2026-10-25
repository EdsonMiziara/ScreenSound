using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScreenSound.API.Request;
using ScreenSound.API.Requests;
using ScreenSound.API.Response;
using ScreenSound.API.Seguranca;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using ScreenSound.Shared.Data.Banco; 
using ScreenSound.Shared.Data.Modelos;
using ScreenSound.Shared.Modelos.Auxiliar;
using ScreenSound.Web.Response;
using System.Security.Claims;

namespace ScreenSound.API.Endpoints;

public static class MusicaExtensions
{
    public static void AddEndPointsMusicas(this WebApplication app)
    {
        var grupo = app.MapGroup("/Musicas")
            .RequireCors("wasm")
            .RequireAuthorization()
            .WithTags("Musicas");

        grupo.MapGet("/", async ([FromServices] DAL<Musica> dal) =>
        {
            var lista = await dal.ListarComoQueryable()
                .Include(m => m.Artista)
                .Include(m => m.Generos)
                .Include(m => m.Notas)
                .ToListAsync();

            var resposta = EntityListToResponseList(lista);
            return Results.Ok(resposta);
        });

        grupo.MapGet("/{nome}", async ([FromServices] DAL<Musica> dal, string nome) =>
        {
            var musica = await dal.RecuperarComIncludeAsync(
                a => a.Nome.ToUpper().Equals(nome.ToUpper()),
                m => m.Artista!, m => m.Generos, m => m.Notas); 

            return musica is null ? Results.NotFound() : Results.Ok(EntityToResponse(musica));
        });

        grupo.MapPost("/", async (
            [FromServices] IUnitOfWork uow,
            [FromServices] DAL<Musica> dal,
            [FromServices] DAL<Genero> dalGenero,
            [FromBody] MusicaRequest musicaRequest) =>
        {
            try
            {
                var musica = new Musica(musicaRequest.nome)
                {
                    ArtistaId = musicaRequest.ArtistaId,
                    AnoLancamento = musicaRequest.anoLancamento,
                    Generos = musicaRequest.Generos is not null
                        ? await GeneroRequestConverter(musicaRequest.Generos, dalGenero, uow)
                        : new List<Genero>(),
                    SpotifyUrl = musicaRequest.SpotifyUrl
                };

                if (!musica.EhValido)
                {
                    return Results.BadRequest(musica.Erros.Sumario);
                }

                dal.Adicionar(musica);
                await uow.CommitAsync();

                return Results.Created();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO AO CADASTRAR MÚSICA: {ex.Message} --- {ex.InnerException?.Message}");
                throw; 
            }
        }).RequireAuthorization(Politicas.Admin);

        grupo.MapPut("/", async (
            [FromServices] IUnitOfWork uow, 
            [FromServices] DAL<Musica> dal,
            [FromServices] DAL<Genero> dalGenero,
            [FromBody] MusicaRequestEdit musicaRequestEdit) =>
        {
            var musicaAAtualizar = await dal.RecuperarComIncludeAsync(
                a => a.Id == musicaRequestEdit.Id,
                a => a.Generos); 

            if (musicaAAtualizar is null)
            {
                return Results.NotFound();
            }

            musicaAAtualizar.Nome = musicaRequestEdit.nome;
            musicaAAtualizar.AnoLancamento = musicaRequestEdit.anoLancamento;
            musicaAAtualizar.ArtistaId = musicaRequestEdit.ArtistaId;
            musicaAAtualizar.Generos.Clear();
            musicaAAtualizar.SpotifyUrl = musicaRequestEdit.SpotifyUrl is not null ? musicaRequestEdit.SpotifyUrl : musicaAAtualizar.SpotifyUrl;

            if (musicaRequestEdit.Generos is not null)
            {
                var novosGeneros = await GeneroRequestConverter(musicaRequestEdit.Generos, dalGenero, uow);
                foreach (var genero in novosGeneros)
                {
                    musicaAAtualizar.Generos.Add(genero);
                }
            }

            if (!musicaAAtualizar.EhValido)
            {
                return Results.BadRequest(musicaAAtualizar.Erros.Sumario);
            }

            await uow.CommitAsync(); 

            return Results.Ok();
        }).RequireAuthorization(Politicas.Admin);

        grupo.MapDelete("/{id}", async (
            [FromServices] IUnitOfWork uow, 
            [FromServices] DAL<Musica> dal,
            int id) =>
        {
            var musicaADeletar = await dal.RecuperarPorAsync(a => a.Id == id);
            if (musicaADeletar is null) return Results.NotFound(); 

            dal.Deletar(musicaADeletar);
            await uow.CommitAsync(); 

            return Results.NoContent();
        }).RequireAuthorization(Politicas.Admin);

        grupo.MapPost("/Avaliacao/{MusicaId}", async ( 
            HttpContext context,
            [FromServices] IUnitOfWork uow,
            [FromBody] AvaliacaoRequest Request,
            [FromServices] DAL<Musica> dalMusica,
            [FromServices] DAL<PessoaComAcesso> dalPessoa,
            int MusicaId) =>
        {
            var musica = await dalMusica.RecuperarComIncludeAsync(
                a => a.Id == MusicaId, m => m.Notas);

            if (musica is null)
            {
                return Results.NotFound();
            }

            var email = context.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                ?? throw new InvalidOperationException("Pessoa não está conectada");

            var pessoa = await dalPessoa.RecuperarPorAsync(a => a.Email.Equals(email)) 
                ?? throw new InvalidOperationException("Pessoa não encontrada");

            var avaliacao = musica.Notas
                .FirstOrDefault(a => a.PessoaId == pessoa.Id);

            if (avaliacao is null)
            {
                musica.AdicionarNota(pessoa.Id, Request.Nota);
            }
            else
            {
                avaliacao.Nota = Request.Nota;
            }

            await uow.CommitAsync();

            return Results.Created();
        });

        grupo.MapGet("{id}/avaliacao", async (
            int id,
            HttpContext context,
            [FromServices] DAL<Musica> dalMusica,
            [FromServices] DAL<PessoaComAcesso> dalPessoa
           ) =>
        {
            var musica = await dalMusica.RecuperarComIncludeAsync(
                a => a.Id == id, m => m.Notas);

            if (musica is null) return Results.NotFound();

            var email = context.User.Claims
                .FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value
                ?? throw new InvalidOperationException("Não foi encontrado o email da pessoa logada");

            var pessoa = await dalPessoa.RecuperarPorAsync(p => p.Email!.Equals(email))
                ?? throw new InvalidOperationException("Não foi encontrado o email da pessoa logada");

            var avaliacao = musica
                .Notas
                .FirstOrDefault(a => a.PessoaId == pessoa.Id);

            if (avaliacao is null)
            {
                return Results.Ok(new AvaliacaoResponse(id, pessoa.Id, 0));
            }
            else
            {
                return Results.Ok(new AvaliacaoResponse(id, pessoa.Id, avaliacao.Nota));
            }
        });
    }


    /// <summary>
    /// Converte GeneroRequest em entidade Genero, reutilizando ou criando.
    /// Refatorado para ser async e usar RecuperarParaEdicaoAsync para rastrear entidades.
    /// </summary>
    /// <summary>
    /// Converte GeneroRequest em entidade Genero, reutilizando ou criando e salvando na hora.
    /// </summary>
    private static async Task<ICollection<Genero>> GeneroRequestConverter(
        ICollection<GeneroRequest> generos,
        DAL<Genero> dalGenero,
        IUnitOfWork uow)
    {
        var listaDeGeneros = new List<Genero>();
        bool temNovoGenero = false;

        foreach (var item in generos)
        {
            var nomeItem = item.Nome.Trim().ToLower();
            var generoExistente = await dalGenero.RecuperarParaEdicaoAsync(
                g => g.Nome!.ToLower() == nomeItem);

            if (generoExistente is not null)
            {
                listaDeGeneros.Add(generoExistente);
            }
            else
            {
                var novoGenero = RequestToEntity(item);
                dalGenero.Adicionar(novoGenero);
                listaDeGeneros.Add(novoGenero);
                temNovoGenero = true;
            }
        }

        if (temNovoGenero)
        {
            await uow.CommitAsync();
        }

        return listaDeGeneros;
    }


    private static ICollection<GeneroResponse> EntityListToResponseListGenero(IEnumerable<Genero> generolist)
    {
        return generolist.Select(a => EntityToResponseGenero(a)).ToList();
    }
    private static GeneroResponse EntityToResponseGenero(Genero genero)
    {
        return new GeneroResponse(genero.Nome!, genero.Descricao, genero.Id);
    }
    private static Genero RequestToEntity(GeneroRequest genero)
    {
        return new Genero()
        {
            Nome = genero.Nome,
            Descricao = genero.DescricaoGenero
        };
    }

    private static ICollection<MusicaResponse> EntityListToResponseList(IEnumerable<Musica> musicaList)
    {
        return musicaList.Select(a => EntityToResponse(a)).ToList();
    }

    internal static MusicaResponse EntityToResponse(Musica musica)
    {
        var generos = musica.Generos is not null
            ? EntityListToResponseListGenero(musica.Generos)
            : new List<GeneroResponse>();

        var artistaId = musica.Artista?.Id ?? 0;
        var artistaNome = musica.Artista?.Nome ?? "Desconhecido";

        return new MusicaResponse(musica.Id, musica.Nome, artistaId, artistaNome, generos, musica.SpotifyUrl)
        {
            AnoLancamento = musica.AnoLancamento,
            Classificacao = musica.Notas?.Select(a => a.Nota).DefaultIfEmpty(0).Average() ?? 0
        };
    }
}