using ScreenSound.Web.Requests;
using ScreenSound.Web.Response;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

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
            // Log da URL antes da requisição
            Console.WriteLine($"URL da requisição: {_httpClient.BaseAddress}Artistas/");

            var response = await _httpClient.GetAsync("artistas/");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Erro HTTP: {response.StatusCode}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📦 Conteúdo bruto da resposta: {content}");

            var artistas = JsonSerializer.Deserialize<ICollection<ArtistaResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Console.WriteLine($"✅ Artistas desserializados: {artistas?.Count ?? 0}");

            return artistas;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao buscar artistas: {ex.Message}");
            return null;
        }
    }

    public async Task<ArtistaResponse?> GetArtistaPorNomeAsync(string nome)
    {
        return await _httpClient.GetFromJsonAsync<ArtistaResponse>($"Artistas/{nome}");
    }
    public async Task AddArtistaAsync(ArtistaRequest artista)
    {
        await _httpClient.PostAsJsonAsync("Artistas/", artista);
    }
    public async Task<ArtistaResponse?> GetArtistaPorIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<ArtistaResponse>($"Artistas/RecuperaPor/{id}");
    }
    public async Task DeleteArtistaAsync(int id)
    {
        await _httpClient.DeleteAsync($"Artistas/{id}");
    }
    public async Task UpdateArtistaAsync(ArtistaRequestEdit artista)
    {
        await _httpClient.PutAsJsonAsync("Artistas/", artista);
    }
}