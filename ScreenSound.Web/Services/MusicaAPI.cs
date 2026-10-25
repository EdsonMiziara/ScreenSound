using ScreenSound.Web.Requests;
using ScreenSound.Web.Response;
using System.Net.Http.Json;

namespace ScreenSound.Web.Services;

public class MusicaAPI
{
    private readonly HttpClient _httpClient;

    public MusicaAPI(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("API");
    }

    public async Task<ICollection<MusicaResponse>> GetMusicasAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<MusicaResponse>>("Musicas") ?? new List<MusicaResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar músicas: {ex.Message}");
            return new List<MusicaResponse>();
        }
    }

    public async Task AddMusicaAsync(MusicaRequest musicaRequest)
    {
        var response = await _httpClient.PostAsJsonAsync("Musicas", musicaRequest);
        await response.GarantirSucessoAsync();
    }

    public async Task DeleteMusicaAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"Musicas/{id}");
        await response.GarantirSucessoAsync();
    }

    public async Task<MusicaResponse?> GetMusicaPorNomeAsync(string nome)
    {
        var response = await _httpClient.GetAsync($"Musicas/{Uri.EscapeDataString(nome)}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;

        await response.GarantirSucessoAsync();
        return await response.Content.ReadFromJsonAsync<MusicaResponse>();
    }

    public async Task UpdateMusicaAsync(MusicaRequestEdit musicaRequest)
    {
        var response = await _httpClient.PutAsJsonAsync("Musicas", musicaRequest);
        await response.GarantirSucessoAsync();
    }

    public async Task AvaliaMusicaAsync(int musicaId, int nota)
    {
        var response = await _httpClient.PostAsJsonAsync($"Musicas/Avaliacao/{musicaId}", new { nota });
        await response.GarantirSucessoAsync();
    }

    public async Task<AvaliacaoResponse?> GetAvaliacaoMusicaAsync(int musicaId)
    {
        return await _httpClient.GetFromJsonAsync<AvaliacaoResponse>($"Musicas/{musicaId}/avaliacao");
    }
}
