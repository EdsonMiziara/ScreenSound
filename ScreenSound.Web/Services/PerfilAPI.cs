using ScreenSound.Web.Response;
using System.Net.Http.Json;

namespace ScreenSound.Web.Services;

public class PerfilAPI
{
    private readonly HttpClient _httpClient;

    public PerfilAPI(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("API");
    }

    public async Task<PerfilResponse?> GetAsync()
    {
        var response = await _httpClient.GetAsync("Perfil");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PerfilResponse>();
    }

    public async Task<PerfilResponse?> AtualizarAsync(string? nomeExibicao, string? bio)
    {
        var response = await _httpClient.PutAsJsonAsync("Perfil", new { nomeExibicao, bio });
        await response.GarantirSucessoAsync();
        return await response.Content.ReadFromJsonAsync<PerfilResponse>();
    }

    public async Task<PerfilResponse?> AtualizarFotoAsync(string fotoBase64)
    {
        var response = await _httpClient.PutAsJsonAsync("Perfil/Foto", new { fotoBase64 });
        await response.GarantirSucessoAsync();
        return await response.Content.ReadFromJsonAsync<PerfilResponse>();
    }

    public async Task<PerfilResponse?> RemoverFotoAsync()
    {
        var response = await _httpClient.DeleteAsync("Perfil/Foto");
        await response.GarantirSucessoAsync();
        return await response.Content.ReadFromJsonAsync<PerfilResponse>();
    }

    public async Task TrocarSenhaAsync(string senhaAtual, string novaSenha)
    {
        var response = await _httpClient.PostAsJsonAsync("Perfil/Senha", new { senhaAtual, novaSenha });
        await response.GarantirSucessoAsync();
    }

    public async Task<List<UsuarioResponse>> GetUsuariosAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<UsuarioResponse>>("Usuarios") ?? new();
    }

    public async Task DefinirAdminAsync(int usuarioId, bool admin)
    {
        var response = await _httpClient.PutAsJsonAsync($"Usuarios/{usuarioId}/Admin", new { admin });
        await response.GarantirSucessoAsync();
    }
}
