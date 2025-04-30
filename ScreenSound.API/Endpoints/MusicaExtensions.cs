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
        app.MapGet("/Musicas", ([FromServices] DAL<Musica> dal) =>
        {
            return Results.Ok(dal.Listar());
        });

        app.MapGet("/Musicas/{nome}", ([FromServices] DAL<Musica> dal, string nome) =>
        {
            var musica = dal.RecuperarPor(a => a.Nome.ToUpper().Equals(nome.ToUpper()));
            if (musica is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(EntityToResponse(musica));

        });

        app.MapPost("/Musicas", ([FromServices] DAL<Musica> dal, [FromServices] DAL <Genero> dalGenero, [FromBody] MusicaRequest musicaRequest) =>
        {
            var musica = new Musica(musicaRequest.nome)
            {
                ArtistaId = musicaRequest.ArtistaId,
                AnoLancamento = musicaRequest.anoLancamento,
                Generos = musicaRequest.Generos is not null ? GeneroRequestConverter(musicaRequest.Generos, dalGenero) :
        new List<Genero>()

            };
            dal.Adicionar(musica);
            return Results.Ok();
        });

        app.MapDelete("/Musicas/{id}", ([FromServices] DAL<Musica> dal, int id) => {
            var musica = dal.RecuperarPor(a => a.Id == id);
            if (musica is null)
            {
                return Results.NotFound();
            }
            dal.Deletar(musica);
            return Results.NoContent();

        });

        app.MapPut("/Musicas", ([FromServices] DAL<Musica> dal,[FromServices] DAL<Genero> dalGenero, [FromBody] MusicaRequestEdit musicaRequestEdit ) => {
            var musicaAAtualizar = dal.RecuperarComInclude(a => a.Id == musicaRequestEdit.Id, a => ((Musica)(object)a).Generos  // cast necessário por generic
);
            if (musicaAAtualizar is null)
            {
                return Results.NotFound();
            }
            musicaAAtualizar.Nome = musicaRequestEdit.nome;
            musicaAAtualizar.AnoLancamento = musicaRequestEdit.anoLancamento;
            musicaAAtualizar.ArtistaId = musicaRequestEdit.ArtistaId;
            musicaAAtualizar.Generos.Clear(); // <- Isso remove as associações antigas

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

    }

    private static ICollection<Genero> GeneroRequestConverter(ICollection<GeneroRequest> generos,DAL<Genero> dalGenero)
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
