using Microsoft.AspNetCore.Mvc;
using ScreenSound.API.Request;
using ScreenSound.API.Requests;
using ScreenSound.API.Response;
using ScreenSound.Banco;
using ScreenSound.Modelos;

namespace ScreenSound.API.Endpoints;

public static class MusicaExtensions
{
    public static void AddEndPointsMusicas(this WebApplication app)
    {
        var grupo = app.MapGroup("/Musicas").RequireCors("wasm");

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

            dal.Atualizar(musicaAAtualizar);
            return Results.Ok();
        });

        grupo.MapDelete("/{id}", ([FromServices] DAL<Musica> dal, int id) =>
        {
            var musicaADeletar = dal.RecuperarPor(a => a.Id == id);
            dal.Deletar(musicaADeletar!);
            return musicaADeletar is null ? Results.NotFound() : Results.NoContent();
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
        return new MusicaResponse(musica.Id, musica.Nome!, musica.Artista!.Id, musica.Artista.Nome);
    }
}