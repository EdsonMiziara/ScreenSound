using Microsoft.AspNetCore.Mvc;
using ScreenSound.API.Request;
using ScreenSound.API.Requests;
using ScreenSound.API.Response;
using ScreenSound.Banco;
using ScreenSound.Modelos;
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

        grupo.MapGet("/", ([FromServices] DAL<Album> dal) =>
        {
            var lista = dal.Listar();
            var resposta = EntityListToResponseList(lista);

            return Results.Ok(resposta);
        });

        grupo.MapGet("/{Id}", (int Id, [FromServices] DAL<Album> dal) =>
        {
            var album = dal.RecuperarPor(a => a.Id == Id);

            return album is null ? Results.NotFound() : Results.Ok(EntityToResponse(album));
        });

        grupo.MapGet("/{Nome}", (string Nome, [FromServices] DAL<Album> dal) =>
        {
            var album = dal.RecuperarPor(a => a.Nome.ToUpper().Equals(Nome.ToUpper()));

            return album is null ? Results.NotFound() : Results.Ok(EntityToResponse(album));
        });

        grupo.MapPost("/", ([FromServices] DAL<Album> dal, [FromBody] AlbumRequest albumRequest, [FromServices] DAL<Artista> artistaDAL, [FromServices] DAL<Genero> generoDAL, [FromServices] DAL<Musica> musicaDAL) =>
        {
            var album = new Album(albumRequest.Nome)
            {
                ArtistaId = albumRequest.ArtistaId,
                Artista = artistaDAL.RecuperarPor(a => a.Id == albumRequest.ArtistaId),
                Musicas = albumRequest.Musicas is not null ? MusicaRequestConverter(albumRequest.Musicas!, musicaDAL, generoDAL) : new List<Musica>()
            };

            dal.Adicionar(album);
            return Results.Ok();
        });

        grupo.MapPut("/", ([FromServices] DAL<Album> dal, [FromServices] DAL<Artista> artistaDAL, [FromBody] AlbumRequestEdit albumRequestEdit) =>
        {
            var albumAAtualizar = dal.RecuperarComInclude(a => a.Id == albumRequestEdit.Id, a => ((Album)(object)a).Musicas);

            if (albumAAtualizar is null)
            {
                return Results.NotFound();
            }

            albumAAtualizar.Nome = albumRequestEdit.Nome;
            albumAAtualizar.ArtistaId = albumRequestEdit.ArtistaId;
            albumAAtualizar.Musicas = albumRequestEdit.Musicas;
            albumAAtualizar.Artista = artistaDAL.RecuperarPor(a => a.Id == albumRequestEdit.ArtistaId);

            dal.Atualizar(albumAAtualizar);
            return Results.Ok();
        });

        grupo.MapDelete("/{Id}", ([FromServices] DAL<Album> dal, int Id) =>
        {
            var albumADeletar = dal.RecuperarPor(a => a.Id == Id);
            dal.Deletar(albumADeletar!);

            return albumADeletar is null ? Results.NotFound() : Results.NoContent();

        });

        grupo.MapPost("/Avaliacao/{AlbumId}", (HttpContext context,
            [FromBody] AvaliacaoRequest Request,
            [FromServices] DAL<Album> dalAlbum,
            [FromServices] DAL<PessoaComAcesso> dalPessoa,
            int AlbumId) =>
        {
            var album = dalAlbum.RecuperarPor(a => a.Id == AlbumId);
            if (album is null)
            {
                return Results.NotFound();
            }

            var email = context.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                ?? throw new InvalidOperationException("Pessoa não está conectada");

            var pessoa = dalPessoa.RecuperarPor(a => a.Email!.Equals(email))
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

            dalAlbum.Atualizar(album);

            return Results.Created();
        });

        grupo.MapGet("{id}/avaliacao", (
            int id,
            HttpContext context,
            [FromServices] DAL<Album> dalAlbum,
            [FromServices] DAL<PessoaComAcesso> dalPessoa
         ) =>
        {
            var album = dalAlbum.RecuperarPor(a => a.Id == id);
            if (album is null) return Results.NotFound();
            var email = context.User.Claims
                .FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value
                ?? throw new InvalidOperationException("Não foi encontrado o email da pessoa logada");

            var pessoa = dalPessoa.RecuperarPor(p => p.Email!.Equals(email))
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
    private static ICollection<Musica> MusicaRequestConverter(ICollection<MusicaRequest> musicas, DAL<Musica> musicaDAL, DAL<Genero> generoDAL)
    {
        var listaDeMusicas = new List<Musica>();

        var todasMusicas = musicaDAL.Listar();

        foreach (var item in musicas)
        {
            var musicaExistente = todasMusicas
                .FirstOrDefault(g => g.Nome!.Equals(item.nome, StringComparison.OrdinalIgnoreCase));

            if (musicaExistente is not null)
            {
                musicaDAL.Attach(musicaExistente);
                listaDeMusicas.Add(musicaExistente);
            }
            else
            {
                var novoGenero = RequestToEntity(item, generoDAL);
                musicaDAL.Adicionar(novoGenero); 
                listaDeMusicas.Add(novoGenero);
            }
        }

        return listaDeMusicas;
    }
    private static Musica RequestToEntity(MusicaRequest musica, DAL<Genero> generoDAL)
    {
        var generos = GeneroRequestConverter(musica.Generos!, generoDAL);

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
    private static List<Genero> GeneroRequestConverter(ICollection<GeneroRequest> generos, DAL<Genero> generoDAL)
    {
        var listaDeGeneros = new List<Genero>();

        // Carregue todos os gêneros existentes de uma vez para evitar múltiplas queries
        var todosGeneros = generoDAL.Listar();

        foreach (var item in generos)
        {
            var generoExistente = todosGeneros
                .FirstOrDefault(g => g.Nome!.Equals(item.Nome, StringComparison.OrdinalIgnoreCase));

            if (generoExistente is not null)
            {
                // Já existe: assegura que está trackeado
                generoDAL.Attach(generoExistente);
                listaDeGeneros.Add(generoExistente);
            }
            else
            {
                // Criar novo e adicionar ao banco com persistência
                var novoGenero = RequestToEntityGenero(item);
                generoDAL.Adicionar(novoGenero); // Insere no banco
                listaDeGeneros.Add(novoGenero);
            }
        }

        return listaDeGeneros;
    }


}
