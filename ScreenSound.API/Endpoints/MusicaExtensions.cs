using Microsoft.AspNetCore.Mvc;
using ScreenSound.API.Request;
using ScreenSound.API.Requests;
using ScreenSound.API.Response;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using ScreenSound.Shared.Data.Modelos;
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

        grupo.MapGet("/", ([FromServices] DAL<Musica> dal) =>
        {
            var lista = dal.Listar();
            var resposta = EntityListToResponseList(lista);
            return Results.Ok(resposta);
        });

        grupo.MapGet("/{nome}", ([FromServices] DAL<Musica> dal, string nome) =>
        {
            var musica = dal.RecuperarPor(a => a.Nome.ToUpper().Equals(nome.ToUpper()));
            return musica is null ? Results.NotFound() : Results.Ok(EntityToResponse(musica));
        });

        grupo.MapPost("/", ([FromServices] DAL<Musica> dal, [FromServices] DAL<Genero> dalGenero, [FromBody] MusicaRequest musicaRequest) =>
        {
            var musica = new Musica(musicaRequest.nome)
            {
                ArtistaId = musicaRequest.ArtistaId,
                AnoLancamento = musicaRequest.anoLancamento,
                Generos = musicaRequest.Generos is not null
                    ? GeneroRequestConverter(musicaRequest.Generos, dalGenero)
                    : new List<Genero>()
            };
            if (!musica.EhValido)
            {
                return Results.BadRequest(musica.Erros.Sumario);
            }
            dal.Adicionar(musica);
            return Results.Ok();
        });

        grupo.MapPut("/", ([FromServices] DAL<Musica> dal, [FromServices] DAL<Genero> dalGenero, [FromBody] MusicaRequestEdit musicaRequestEdit) =>
        {
            var musicaAAtualizar = dal.RecuperarComInclude(a => a.Id == musicaRequestEdit.Id, a => ((Musica)(object)a).Generos);

            if (musicaAAtualizar is null)
            {
                return Results.NotFound();
            }

            musicaAAtualizar.Nome = musicaRequestEdit.nome;
            musicaAAtualizar.AnoLancamento = musicaRequestEdit.anoLancamento;
            musicaAAtualizar.ArtistaId = musicaRequestEdit.ArtistaId;
            musicaAAtualizar.Generos.Clear();

            if (musicaRequestEdit.Generos is not null)
            {
                var novosGeneros = GeneroRequestConverter(musicaRequestEdit.Generos, dalGenero);
                foreach (var genero in novosGeneros)
                {
                    musicaAAtualizar.Generos.Add(genero);
                }
            }
            if (!musicaAAtualizar.EhValido)
            {
                return Results.BadRequest(musicaAAtualizar.Erros.Sumario);
            }

            dal.Atualizar(musicaAAtualizar);
            return Results.Ok();
        });

        grupo.MapDelete("/{id}", ([FromServices] DAL<Musica> dal, int id) =>
        {
            var musicaADeletar = dal.RecuperarPor(a => a.Id == id);
            dal.Deletar(musicaADeletar!);
            return musicaADeletar is null ? Results.NotFound() : Results.NoContent();
        });

        grupo.MapPost("/Avaliacao/{MusicaId}", (HttpContext context,
            [FromBody] AvaliacaoRequest Request,
            [FromServices] DAL<Musica> dalMusica,
            [FromServices] DAL<PessoaComAcesso> dalPessoa,
            int MusicaId) =>
        {
            var musica = dalMusica.RecuperarPor(a => a.Id == MusicaId);
            if (musica is null)
            {
                return Results.NotFound();
            }

            var email = context.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                ?? throw new InvalidOperationException("Pessoa não está conectada");

            var pessoa = dalPessoa.RecuperarPor(a => a.Email.Equals(email))
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

            dalMusica.Atualizar(musica);

            return Results.Created();
        });

        grupo.MapGet("{id}/avaliacao", (
            int id,
            HttpContext context,
            [FromServices] DAL<Musica> dalMusica,
            [FromServices] DAL<PessoaComAcesso> dalPessoa
         ) =>
            {
                var musica = dalMusica.RecuperarPor(a => a.Id == id);
                if (musica is null) return Results.NotFound();
                var email = context.User.Claims
                    .FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value
                    ?? throw new InvalidOperationException("Não foi encontrado o email da pessoa logada");

                var pessoa = dalPessoa.RecuperarPor(p => p.Email!.Equals(email))
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


    private static ICollection<Genero> GeneroRequestConverter(ICollection<GeneroRequest> generos, DAL<Genero> dalGenero)
    {
        var listaDeGeneros = new List<Genero>();

        // Carregue todos os gêneros existentes de uma vez para evitar múltiplas queries
        var todosGeneros = dalGenero.Listar();

        foreach (var item in generos)
        {
            var generoExistente = todosGeneros
                .FirstOrDefault(g => g.Nome!.Equals(item.Nome, StringComparison.OrdinalIgnoreCase));

            if (generoExistente is not null)
            {
                // Já existe: assegura que está trackeado
                dalGenero.Attach(generoExistente);
                listaDeGeneros.Add(generoExistente);
            }
            else
            {
                // Criar novo e adicionar ao banco com persistência
                var novoGenero = RequestToEntity(item);
                dalGenero.Adicionar(novoGenero); // Insere no banco
                listaDeGeneros.Add(novoGenero);
            }
        }

        return listaDeGeneros;
    }


    private static ICollection<GeneroResponse> EntityListToResponseListGenero(IEnumerable<Genero> generolist)
    {
        return generolist.Select(a => EntityToResponseGenero(a)).ToList();
    }
    private static GeneroResponse EntityToResponseGenero(Genero genero)
    {
        return new GeneroResponse( genero.Nome!, genero.Descricao, genero.Id);
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

    private static MusicaResponse EntityToResponse(Musica musica)
    {
        var generos =  musica.Generos is not null ? EntityListToResponseListGenero(musica.Generos) : new List<GeneroResponse>();
        return new MusicaResponse(musica.Id, musica.Nome, musica.Artista.Id, musica.Artista.Nome, generos) 
        {
            Classificacao = musica.Notas.Select(a => a.Nota).DefaultIfEmpty(0).Average()
        };
    }
}