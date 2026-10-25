using ScreenSound.Shared.Modelos.Auxiliar;
using System.Net.Http.Json;

namespace ScreenSound.Web.Services;

/// <summary>
/// Busca a capa (thumbnail) das faixas pelo oEmbed público do Spotify, com cache em memória.
/// </summary>
public class CapaService
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, Task<string?>> cache = new();

    public CapaService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("Spotify");
    }

    public Task<string?> ObterAsync(string? spotifyUrl)
    {
        var canonico = SpotifyLink.ParaCanonico(spotifyUrl);
        if (canonico is null) return Task.FromResult<string?>(null);

        if (!cache.TryGetValue(canonico, out var tarefa))
        {
            tarefa = BuscarAsync(canonico);
            cache[canonico] = tarefa;
        }

        return tarefa;
    }

    private async Task<string?> BuscarAsync(string canonico)
    {
        try
        {
            var oembed = await _httpClient.GetFromJsonAsync<OEmbed>(
                $"https://open.spotify.com/oembed?url={Uri.EscapeDataString(canonico)}");
            return oembed?.thumbnail_url;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private record OEmbed(string? thumbnail_url);
}
