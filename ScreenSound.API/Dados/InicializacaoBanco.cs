using Microsoft.EntityFrameworkCore;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using ScreenSound.Shared.Data.Modelos;

namespace ScreenSound.API.Dados;

public static class InicializacaoBanco
{
    /// <summary>
    /// Aplica as migrations pendentes quando "Banco:AplicarMigracoesAoIniciar" é true
    /// (usado no Docker e, se desejado, no Azure via App Settings).
    /// </summary>
    public static async Task AplicarMigracoesAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>("Banco:AplicarMigracoesAoIniciar")) return;

        using var escopo = app.Services.CreateScope();
        var logger = escopo.ServiceProvider.GetRequiredService<ILogger<ScreenSoundContext>>();
        var contexto = escopo.ServiceProvider.GetRequiredService<ScreenSoundContext>();

        var pendentes = (await contexto.Database.GetPendingMigrationsAsync()).ToList();
        if (pendentes.Count == 0) return;

        logger.LogInformation("Aplicando {Quantidade} migration(s): {Migrations}", pendentes.Count, string.Join(", ", pendentes));
        await contexto.Database.MigrateAsync();
    }

    /// <summary>
    /// Popula um catálogo de demonstração (artistas, faixas com link do Spotify e playlists)
    /// quando "Banco:PopularCatalogoDemo" é true e o banco ainda não tem artistas.
    /// </summary>
    public static async Task PopularCatalogoDemoAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>("Banco:PopularCatalogoDemo")) return;

        using var escopo = app.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<ScreenSoundContext>();
        var logger = escopo.ServiceProvider.GetRequiredService<ILogger<ScreenSoundContext>>();

        try
        {
            if (await contexto.Artistas.AnyAsync()) return;

            var artistas = CatalogoDemo.Artistas.ToDictionary(a => a.Nome, a => new Artista(a.Nome, a.Bio));
            contexto.Artistas.AddRange(artistas.Values);

            var generos = new Dictionary<string, Genero>(StringComparer.OrdinalIgnoreCase);
            var musicas = new Dictionary<string, Musica>();
            foreach (var faixa in CatalogoDemo.Faixas)
            {
                var musica = new Musica(faixa.Nome)
                {
                    Artista = artistas[faixa.Artista],
                    AnoLancamento = faixa.Ano,
                    SpotifyUrl = faixa.SpotifyId is null ? null : $"https://open.spotify.com/track/{faixa.SpotifyId}",
                    Generos = faixa.Generos
                        .Select(g => generos.TryGetValue(g, out var existente) ? existente : generos[g] = new Genero(g, string.Empty))
                        .ToList()
                };
                musicas[faixa.Nome] = musica;
            }
            contexto.Musicas.AddRange(musicas.Values);
            await contexto.SaveChangesAsync();

            // Playlists públicas da conta demo admin, para a seção "Da comunidade"
            var dono = await contexto.Set<PessoaComAcesso>().FirstOrDefaultAsync(p => p.Email == "edson@email.com");
            if (dono is not null)
            {
                foreach (var (nome, descricao, faixas) in CatalogoDemo.Playlists)
                {
                    var playlist = new Playlist(nome, dono.Id) { Descricao = descricao, Publica = true };
                    foreach (var faixa in faixas) playlist.AdicionarMusica(musicas[faixa].Id);
                    contexto.Playlists.Add(playlist);
                }
                await contexto.SaveChangesAsync();
            }

            logger.LogInformation("Catálogo de demonstração criado: {Artistas} artistas e {Faixas} faixas.", artistas.Count, musicas.Count);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Não foi possível popular o catálogo de demonstração.");
        }
    }
}
