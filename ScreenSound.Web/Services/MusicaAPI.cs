using ScreenSound.Modelos;
using ScreenSound.Web.Requests;
using ScreenSound.Web.Response;
using System.Collections;
using System.Net.Http.Json;
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
            Console.WriteLine($"URL da requisição: {_httpClient.BaseAddress}Musicas/");

            var response = await _httpClient.GetAsync("musicas/");

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
    public async Task AddMusicaAsync(MusicaRequest musicaRequest)
    {
        await _httpClient.PostAsJsonAsync("Musicas/", musicaRequest);
    }
    public async Task DeleteMusicaAsync(int id)
    {
        await _httpClient.DeleteAsync($"Musicas/{id}");
    }
    public async Task<MusicaResponse?> GetMusicaPorNomeAsync(string nome)
    {
        return await _httpClient.GetFromJsonAsync<MusicaResponse>($"Musicas/{nome}");
    }
    public async Task UpdateMusicaAsync(MusicaRequestEdit musicaRequest)
    {
        await _httpClient.PutAsJsonAsync($"Musicas/", musicaRequest);
    }

    public async Task AvaliaMusicaAsync(int MusicaId, int nota)
    {
        await _httpClient.PostAsJsonAsync("Musicas/Avaliacao", new { MusicaId, nota });
    }

    public async Task<AvaliacaoResponse?> GetAvaliacaoArtistaAsync(int musicaId)
    {
        return await _httpClient.GetFromJsonAsync<AvaliacaoResponse>($"Musicas/{musicaId}/Avaliacao");
    }
}