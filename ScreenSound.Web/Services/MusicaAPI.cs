
using ScreenSound.Modelos;
using ScreenSound.Web.Response;
using System.Collections;
using System.Text.Json;

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
            // Log da URL antes da requisição
            Console.WriteLine($"URL da requisição: {_httpClient.BaseAddress}Musicas");

            var response = await _httpClient.GetAsync("musicas");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Erro HTTP: {response.StatusCode}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📦 Conteúdo bruto da resposta: {content}");

            var musicas = JsonSerializer.Deserialize<ICollection<MusicaResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Console.WriteLine($"✅ Artistas desserializados: {musicas?.Count ?? 0}");

            return musicas;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao buscar artistas: {ex.Message}");
            return null;
        }
    }
}
