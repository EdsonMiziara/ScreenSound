using ScreenSound.Web.Requests;
using ScreenSound.Web.Response;
using System.Net.Http.Json;

namespace ScreenSound.Web.Services;

public class ArtistaAPI
{
    private readonly HttpClient _httpClient;

    public ArtistaAPI(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("API");
    }

    public async Task<ICollection<ArtistaResponse>?> GetArtistasAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<ArtistaResponse>>("Artistas");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar artistas: {ex.Message}");
            return null;
        }
    }

    public async Task<ArtistaResponse?> GetArtistaPorNomeAsync(string nome)
    {
        var response = await _httpClient.GetAsync($"Artistas/{Uri.EscapeDataString(nome)}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;

        await response.GarantirSucessoAsync();
        return await response.Content.ReadFromJsonAsync<ArtistaResponse>();
    }

    public async Task AddArtistaAsync(ArtistaRequest artista)
    {
        var response = await _httpClient.PostAsJsonAsync("Artistas", artista);
        await response.GarantirSucessoAsync();
    }

    public async Task<ArtistaResponse?> GetArtistaPorIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<ArtistaResponse>($"Artistas/RecuperaPor/{id}");
    }

    public async Task DeleteArtistaAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"Artistas/{id}");
        await response.GarantirSucessoAsync();
    }

    public async Task UpdateArtistaAsync(ArtistaRequestEdit artista)
    {
        var response = await _httpClient.PutAsJsonAsync("Artistas", artista);
        await response.GarantirSucessoAsync();
    }

    public async Task AvaliaArtistaAsync(int artistaId, int nota)
    {
        var response = await _httpClient.PostAsJsonAsync($"Artistas/Avaliacao/{artistaId}", new { nota });
        await response.GarantirSucessoAsync();
    }

    public async Task<AvaliacaoResponse?> GetAvaliacaoArtistaAsync(int artistaId)
    {
        return await _httpClient.GetFromJsonAsync<AvaliacaoResponse>($"Artistas/{artistaId}/Avaliacao");
    }
}
