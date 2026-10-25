using ScreenSound.Web.Requests;
using ScreenSound.Web.Response;
using System.Net.Http.Json;

namespace ScreenSound.Web.Services;

public class PlaylistAPI
{
    private readonly HttpClient _httpClient;

    public PlaylistAPI(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("API");
    }

    public async Task<List<PlaylistResponse>> GetMinhasAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<PlaylistResponse>>("Playlists") ?? new();
    }

    public async Task<List<PlaylistResponse>> GetPublicasAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<PlaylistResponse>>("Playlists/Publicas") ?? new();
    }

    public async Task<PlaylistResponse?> GetPorIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"Playlists/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;

        await response.GarantirSucessoAsync();
        return await response.Content.ReadFromJsonAsync<PlaylistResponse>();
    }

    public async Task<int> CriarAsync(PlaylistRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("Playlists", request);
        await response.GarantirSucessoAsync();

        var criada = await response.Content.ReadFromJsonAsync<PlaylistCriada>();
        return criada?.Id ?? 0;
    }

    public async Task AtualizarAsync(int id, PlaylistRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"Playlists/{id}", request);
        await response.GarantirSucessoAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"Playlists/{id}");
        await response.GarantirSucessoAsync();
    }

    public async Task AdicionarMusicaAsync(int playlistId, int musicaId)
    {
        var response = await _httpClient.PostAsync($"Playlists/{playlistId}/Musicas/{musicaId}", null);
        await response.GarantirSucessoAsync();
    }

    public async Task RemoverMusicaAsync(int playlistId, int musicaId)
    {
        var response = await _httpClient.DeleteAsync($"Playlists/{playlistId}/Musicas/{musicaId}");
        await response.GarantirSucessoAsync();
    }

    public async Task ReordenarAsync(int playlistId, IEnumerable<int> musicaIds)
    {
        var response = await _httpClient.PutAsJsonAsync($"Playlists/{playlistId}/Ordem", new PlaylistOrdemRequest(musicaIds.ToList()));
        await response.GarantirSucessoAsync();
    }

    private record PlaylistCriada(int Id);
}
