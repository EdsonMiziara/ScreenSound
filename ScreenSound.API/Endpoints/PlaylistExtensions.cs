using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScreenSound.API.Requests;
using ScreenSound.API.Response;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using ScreenSound.Shared.Data.Banco;
using ScreenSound.Shared.Data.Modelos;
using System.Security.Claims;

namespace ScreenSound.API.Endpoints;

public static class PlaylistExtensions
{
    public static void AddEndpointsPlaylists(this WebApplication app)
    {
        var grupo = app.MapGroup("/Playlists")
            .RequireCors("wasm")
            .RequireAuthorization()
            .WithTags("Playlists");

        // Playlists da pessoa logada
        grupo.MapGet("/", async (ClaimsPrincipal user, [FromServices] DAL<Playlist> dal, [FromServices] DAL<PessoaComAcesso> dalPessoa) =>
        {
            var pessoaId = ObterPessoaId(user);
            if (pessoaId is null) return Results.Unauthorized();

            var playlists = await ConsultaCompleta(dal)
                .Where(p => p.PessoaId == pessoaId)
                .OrderByDescending(p => p.CriadaEm)
                .ToListAsync();

            var donos = await NomesDosDonos(playlists, dalPessoa);
            return Results.Ok(playlists.Select(p => EntityToResponse(p, pessoaId.Value, donos)).ToList());
        });

        // Playlists públicas da comunidade (inclusive as da pessoa logada)
        grupo.MapGet("/Publicas", async (ClaimsPrincipal user, [FromServices] DAL<Playlist> dal, [FromServices] DAL<PessoaComAcesso> dalPessoa) =>
        {
            var pessoaId = ObterPessoaId(user) ?? 0;

            var playlists = await ConsultaCompleta(dal)
                .Where(p => p.Publica)
                .OrderByDescending(p => p.CriadaEm)
                .ToListAsync();

            var donos = await NomesDosDonos(playlists, dalPessoa);
            return Results.Ok(playlists.Select(p => EntityToResponse(p, pessoaId, donos)).ToList());
        });

        grupo.MapGet("/{id:int}", async (int id, ClaimsPrincipal user, [FromServices] DAL<Playlist> dal, [FromServices] DAL<PessoaComAcesso> dalPessoa) =>
        {
            var pessoaId = ObterPessoaId(user) ?? 0;

            var playlist = await ConsultaCompleta(dal).FirstOrDefaultAsync(p => p.Id == id);
            if (playlist is null || (!playlist.Publica && playlist.PessoaId != pessoaId))
            {
                return Results.NotFound();
            }

            var donos = await NomesDosDonos([playlist], dalPessoa);
            return Results.Ok(EntityToResponse(playlist, pessoaId, donos));
        });

        grupo.MapPost("/", async (
            ClaimsPrincipal user,
            [FromServices] IUnitOfWork uow,
            [FromServices] DAL<Playlist> dal,
            [FromServices] DAL<Musica> dalMusica,
            [FromBody] PlaylistRequest request) =>
        {
            var pessoaId = ObterPessoaId(user);
            if (pessoaId is null) return Results.Unauthorized();

            var playlist = new Playlist(request.Nome?.Trim() ?? string.Empty, pessoaId.Value)
            {
                Descricao = request.Descricao?.Trim(),
                Publica = request.Publica
            };

            if (!playlist.EhValido)
            {
                return Results.BadRequest(playlist.Erros.Sumario);
            }

            if (request.MusicaIds is { Count: > 0 })
            {
                var idsExistentes = await dalMusica.ListarComoQueryable()
                    .Where(m => request.MusicaIds.Contains(m.Id))
                    .Select(m => m.Id)
                    .ToListAsync();

                foreach (var musicaId in request.MusicaIds.Where(idsExistentes.Contains))
                {
                    playlist.AdicionarMusica(musicaId);
                }
            }

            dal.Adicionar(playlist);
            await uow.CommitAsync();

            return Results.Created($"/Playlists/{playlist.Id}", new { playlist.Id });
        });

        grupo.MapPut("/{id:int}", async (
            int id,
            ClaimsPrincipal user,
            [FromServices] IUnitOfWork uow,
            [FromServices] DAL<Playlist> dal,
            [FromBody] PlaylistRequest request) =>
        {
            var pessoaId = ObterPessoaId(user);
            var playlist = await dal.RecuperarParaEdicaoAsync(p => p.Id == id);
            if (playlist is null || playlist.PessoaId != pessoaId) return Results.NotFound();

            if (string.IsNullOrWhiteSpace(request.Nome) || request.Nome.Trim().Length > 100)
            {
                return Results.BadRequest("O nome da playlist deve ter entre 1 e 100 caracteres.");
            }

            playlist.Nome = request.Nome.Trim();
            playlist.Descricao = request.Descricao?.Trim();
            playlist.Publica = request.Publica;

            await uow.CommitAsync();
            return Results.Ok();
        });

        grupo.MapDelete("/{id:int}", async (
            int id,
            ClaimsPrincipal user,
            [FromServices] IUnitOfWork uow,
            [FromServices] DAL<Playlist> dal) =>
        {
            var pessoaId = ObterPessoaId(user);
            var playlist = await dal.RecuperarParaEdicaoAsync(p => p.Id == id);
            if (playlist is null || playlist.PessoaId != pessoaId) return Results.NotFound();

            dal.Deletar(playlist);
            await uow.CommitAsync();
            return Results.NoContent();
        });

        grupo.MapPost("/{id:int}/Musicas/{musicaId:int}", async (
            int id,
            int musicaId,
            ClaimsPrincipal user,
            [FromServices] IUnitOfWork uow,
            [FromServices] DAL<Playlist> dal,
            [FromServices] DAL<Musica> dalMusica) =>
        {
            var pessoaId = ObterPessoaId(user);
            var playlist = await dal.RecuperarComIncludeAsync(p => p.Id == id, p => p.Itens);
            if (playlist is null || playlist.PessoaId != pessoaId) return Results.NotFound();

            if (await dalMusica.RecuperarPorAsync(m => m.Id == musicaId) is null)
            {
                return Results.NotFound("Música não encontrada.");
            }

            if (!playlist.AdicionarMusica(musicaId))
            {
                return Results.Conflict("A música já está nesta playlist.");
            }

            await uow.CommitAsync();
            return Results.Ok();
        });

        grupo.MapDelete("/{id:int}/Musicas/{musicaId:int}", async (
            int id,
            int musicaId,
            ClaimsPrincipal user,
            [FromServices] IUnitOfWork uow,
            [FromServices] DAL<Playlist> dal) =>
        {
            var pessoaId = ObterPessoaId(user);
            var playlist = await dal.RecuperarComIncludeAsync(p => p.Id == id, p => p.Itens);
            if (playlist is null || playlist.PessoaId != pessoaId) return Results.NotFound();

            if (!playlist.RemoverMusica(musicaId)) return Results.NotFound();

            await uow.CommitAsync();
            return Results.NoContent();
        });

        grupo.MapPut("/{id:int}/Ordem", async (
            int id,
            ClaimsPrincipal user,
            [FromServices] IUnitOfWork uow,
            [FromServices] DAL<Playlist> dal,
            [FromBody] PlaylistOrdemRequest request) =>
        {
            var pessoaId = ObterPessoaId(user);
            var playlist = await dal.RecuperarComIncludeAsync(p => p.Id == id, p => p.Itens);
            if (playlist is null || playlist.PessoaId != pessoaId) return Results.NotFound();

            playlist.Reordenar(request.MusicaIds);

            await uow.CommitAsync();
            return Results.Ok();
        });
    }

    private static int? ObterPessoaId(ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private static IQueryable<Playlist> ConsultaCompleta(DAL<Playlist> dal) =>
        dal.ListarComoQueryable()
            .Include(p => p.Itens).ThenInclude(i => i.Musica!).ThenInclude(m => m.Artista)
            .Include(p => p.Itens).ThenInclude(i => i.Musica!).ThenInclude(m => m.Generos)
            .Include(p => p.Itens).ThenInclude(i => i.Musica!).ThenInclude(m => m.Notas)
            .AsSplitQuery();

    private static async Task<Dictionary<int, string>> NomesDosDonos(IEnumerable<Playlist> playlists, DAL<PessoaComAcesso> dalPessoa)
    {
        var ids = playlists.Select(p => p.PessoaId).Distinct().ToList();

        var pessoas = await dalPessoa.ListarComoQueryable()
            .Where(p => ids.Contains(p.Id))
            .Select(p => new { p.Id, p.NomeExibicao, p.UserName, p.Email })
            .ToListAsync();

        // Sem nome de exibição, mostra só a parte antes do @ para não expor o e-mail de outras pessoas
        return pessoas.ToDictionary(
            p => p.Id,
            p => !string.IsNullOrWhiteSpace(p.NomeExibicao) ? p.NomeExibicao : (p.UserName ?? p.Email ?? "anônimo").Split('@')[0]);
    }

    private static PlaylistResponse EntityToResponse(Playlist playlist, int pessoaLogadaId, IReadOnlyDictionary<int, string> donos)
    {
        var musicas = playlist.Itens
            .OrderBy(i => i.Ordem)
            .Where(i => i.Musica is not null)
            .Select(i => MusicaExtensions.EntityToResponse(i.Musica!))
            .ToList();

        return new PlaylistResponse(
            playlist.Id,
            playlist.Nome,
            playlist.Descricao,
            playlist.Publica,
            playlist.PessoaId == pessoaLogadaId,
            donos.GetValueOrDefault(playlist.PessoaId),
            playlist.CriadaEm,
            musicas);
    }
}
