// ScreenSound.API.Endpoints/AlbumExtensions.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Necessário para .ToListAsync() e .Include()
using ScreenSound.API.Request;
using ScreenSound.API.Requests;
using ScreenSound.API.Response;
using ScreenSound.API.Seguranca;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using ScreenSound.Shared.Data.Banco; // Necessário para IUnitOfWork
using ScreenSound.Shared.Data.Modelos;
using System.Security.Claims;

namespace ScreenSound.API.Endpoints;

public static class AlbumExtensions
{
    public static void AddEndpointsAlbuns(this WebApplication app)
    {

        var grupo = app.MapGroup("/Albuns")
            .RequireCors("wasm")
            .RequireAuthorization()
            .WithTags("Albuns");

        grupo.MapGet("/", async ([FromServices] DAL<Album> dal) =>
        {
            var lista = await dal.ListarComoQueryable()
                .Include(a => a.Artista)
                .Include(a => a.Notas)
                .Include(a => a.Musicas)
                .ToListAsync();

            var resposta = EntityListToResponseList(lista);
            return Results.Ok(resposta);
        });

        grupo.MapGet("/{Id}", async (int Id, [FromServices] DAL<Album> dal) =>
        {
            var album = await dal.RecuperarComIncludeAsync(
                a => a.Id == Id,
                a => a.Artista!, a => a.Notas, a => a.Musicas);

            return album is null ? Results.NotFound() : Results.Ok(EntityToResponse(album));
        });

        grupo.MapGet("/{Nome}", async (string Nome, [FromServices] DAL<Album> dal) =>
        {
            var album = await dal.RecuperarComIncludeAsync(
                a => a.Nome.ToUpper().Equals(Nome.ToUpper()),
                a => a.Artista!, a => a.Notas, a => a.Musicas);

            return album is null ? Results.NotFound() : Results.Ok(EntityToResponse(album));
        });

        grupo.MapPost("/", async (
            [FromServices] IUnitOfWork uow,
            [FromServices] DAL<Album> dal,
            [FromBody] AlbumRequest albumRequest,
            [FromServices] DAL<Artista> artistaDAL,
            [FromServices] DAL<Genero> generoDAL,
            [FromServices] DAL<Musica> musicaDAL) =>
        {
            var album = new Album(albumRequest.Nome)
            {
                ArtistaId = albumRequest.ArtistaId,
                Artista = await artistaDAL.RecuperarPorAsync(a => a.Id == albumRequest.ArtistaId),
                Musicas = albumRequest.Musicas is not null
                    ? await MusicaRequestConverter(albumRequest.Musicas!, musicaDAL, generoDAL)
                    : new List<Musica>()
            };

            dal.Adicionar(album);
            await uow.CommitAsync(); 

            return Results.Created();
        }).RequireAuthorization(Politicas.Admin);

        grupo.MapPut("/", async (
            [FromServices] IUnitOfWork uow,
            [FromServices] DAL<Album> dal,
            [FromServices] DAL<Artista> artistaDAL,
            [FromBody] AlbumRequestEdit albumRequestEdit) =>
        {
            var albumAAtualizar = await dal.RecuperarComIncludeAsync(
                a => a.Id == albumRequestEdit.Id, a => a.Musicas);

            if (albumAAtualizar is null)
            {
                return Results.NotFound();
            }

            albumAAtualizar.Nome = albumRequestEdit.Nome;
            albumAAtualizar.ArtistaId = albumRequestEdit.ArtistaId;
            albumAAtualizar.Musicas = albumRequestEdit.Musicas;
            albumAAtualizar.Artista = await artistaDAL.RecuperarPorAsync(a => a.Id == albumRequestEdit.ArtistaId);

            await uow.CommitAsync(); 

            return Results.Ok();
        }).RequireAuthorization(Politicas.Admin);

        grupo.MapDelete("/{Id}", async (
            [FromServices] IUnitOfWork uow,
            [FromServices] DAL<Album> dal,
            int Id) =>
        {
            var albumADeletar = await dal.RecuperarPorAsync(a => a.Id == Id);
            if (albumADeletar is null) return Results.NotFound();

            dal.Deletar(albumADeletar);
            await uow.CommitAsync(); 

            return Results.NoContent();
        }).RequireAuthorization(Politicas.Admin);

        grupo.MapPost("/Avaliacao/{AlbumId}", async (
            HttpContext context,
            [FromServices] IUnitOfWork uow,
            [FromBody] AvaliacaoRequest Request,
            [FromServices] DAL<Album> dalAlbum,
            [FromServices] DAL<PessoaComAcesso> dalPessoa,
            int AlbumId) =>
        {
            var album = await dalAlbum.RecuperarComIncludeAsync(
                a => a.Id == AlbumId, a => a.Notas);

            if (album is null)
            {
                return Results.NotFound();
            }

            var email = context.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                ?? throw new InvalidOperationException("Pessoa não está conectada");

            var pessoa = await dalPessoa.RecuperarPorAsync(a => a.Email!.Equals(email))
                ?? throw new InvalidOperationException("Pessoa não encontrada");

            var avaliacao = album.Notas
                .FirstOrDefault(a => a.PessoaId == pessoa.Id);

            if (avaliacao is null)
            {
                album.AdicionarNota(pessoa.Id, Request.Nota);
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
            [FromServices] DAL<Album> dalAlbum,
            [FromServices] DAL<PessoaComAcesso> dalPessoa
           ) =>
        {
            var album = await dalAlbum.RecuperarComIncludeAsync(a => a.Id == id, a => a.Notas);
            if (album is null) return Results.NotFound();

            var email = context.User.Claims
                .FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value
                ?? throw new InvalidOperationException("Não foi encontrado o email da pessoa logada");

            var pessoa = await dalPessoa.RecuperarPorAsync(p => p.Email!.Equals(email))
                ?? throw new InvalidOperationException("Não foi encontrado o email da pessoa logada");

            var avaliacao = album
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


    private static ICollection<AlbumResponse> EntityListToResponseList(IEnumerable<Album> lista)
    {
        return lista.Select(a => EntityToResponse(a)).ToList();
    }

    private static AlbumResponse EntityToResponse(Album album)
    {
        var artista = EntityArtistaToResponse(album.Artista!);
        return new AlbumResponse(album.Id, album.Nome, album.Musicas, artista)
        {
            Classificacao = album.Notas.Select(a => a.Nota).DefaultIfEmpty(0).Average()
        };
    }

    private static ArtistaResponse EntityArtistaToResponse(Artista artista)
    {
        return new ArtistaResponse(artista.Id, artista.Nome, artista.Bio, artista.FotoPerfil);
    }

    // --- Métodos Helper Refatorados para Async ---
    // (Necessário pois o DAL agora é async e o UoW precisa
    //  que as entidades sejam rastreadas corretamente)

    /// <summary>
    /// Converte MusicaRequest em entidade Musica, reutilizando ou criando.
    /// Refatorado para usar RecuperarParaEdicaoAsync para rastrear entidades existentes.
    /// </summary>
    private static async Task<ICollection<Musica>> MusicaRequestConverter(
        ICollection<MusicaRequest> musicas,
        DAL<Musica> musicaDAL,
        DAL<Genero> generoDAL)
    {
        var listaDeMusicas = new List<Musica>();

        foreach (var item in musicas)
        {
            var musicaExistente = await musicaDAL.RecuperarParaEdicaoAsync(
                g => g.Nome!.Equals(item.nome, StringComparison.OrdinalIgnoreCase));

            if (musicaExistente is not null)
            {

                listaDeMusicas.Add(musicaExistente);
            }
            else
            {
                var novaMusica = await RequestToEntity(item, generoDAL);
                musicaDAL.Adicionar(novaMusica); 
                listaDeMusicas.Add(novaMusica);
            }
        }

        return listaDeMusicas;
    }

    /// <summary>
    /// Converte MusicaRequest em uma *nova* entidade Musica.
    /// </summary>
    private static async Task<Musica> RequestToEntity(MusicaRequest musica, DAL<Genero> generoDAL)
    {
        var generos = await GeneroRequestConverter(musica.Generos!, generoDAL);
        return new Musica(musica.nome, musica.anoLancamento, musica.ArtistaId, generos);
    }

    private static Genero RequestToEntityGenero(GeneroRequest genero)
    {
        return new Genero()
        {
            Nome = genero.Nome,
            Descricao = genero.DescricaoGenero
        };
    }

    /// <summary>
    /// Converte GeneroRequest em entidade Genero, reutilizando ou criando.
    /// Refatorado para usar RecuperarParaEdicaoAsync para rastrear entidades existentes.
    /// </summary>
    private static async Task<List<Genero>> GeneroRequestConverter(
        ICollection<GeneroRequest> generos,
        DAL<Genero> generoDAL)
    {
        var listaDeGeneros = new List<Genero>();

        foreach (var item in generos)
        {
            var generoExistente = await generoDAL.RecuperarParaEdicaoAsync(
                g => g.Nome!.Equals(item.Nome, StringComparison.OrdinalIgnoreCase));

            if (generoExistente is not null)
            {
                listaDeGeneros.Add(generoExistente);
            }
            else
            {
                var novoGenero = RequestToEntityGenero(item);
                generoDAL.Adicionar(novoGenero);
                listaDeGeneros.Add(novoGenero);
            }
        }

        return listaDeGeneros;
    }
}