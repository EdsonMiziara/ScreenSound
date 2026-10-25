using Microsoft.AspNetCore.Components.Authorization;
using ScreenSound.Web.Response;
using System.Net.Http.Json;
using System.Security.Claims;

namespace ScreenSound.Web.Services;

public class AuthAPI(IHttpClientFactory factory) : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient = factory.CreateClient("API");

    private bool autenticado = false;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        autenticado = false;
        var pessoa = new ClaimsPrincipal();

        try
        {
            var response = await _httpClient.GetAsync("Perfil");

            if (response.IsSuccessStatusCode)
            {
                var perfil = await response.Content.ReadFromJsonAsync<PerfilResponse>();
                var dados = new List<Claim>
                {
                    new(ClaimTypes.Name, perfil!.Email),
                    new(ClaimTypes.Email, perfil.Email),
                    new(ClaimTypes.NameIdentifier, perfil.Id.ToString()),
                    new(ClaimsScreenSound.Nome, perfil.NomePublico),
                    new(ClaimsScreenSound.Demo, perfil.ContaDemo.ToString())
                };
                if (!string.IsNullOrEmpty(perfil.FotoPerfil)) dados.Add(new Claim(ClaimsScreenSound.Foto, perfil.FotoPerfil));
                dados.AddRange(perfil.Papeis.Select(papel => new Claim(ClaimTypes.Role, papel)));

                var identity = new ClaimsIdentity(dados, "Cookies");
                pessoa = new ClaimsPrincipal(identity);
                autenticado = true;
            }
        }
        catch (HttpRequestException)
        {
            // API fora do ar: segue como anônimo para que a landing page continue acessível
        }

        return new AuthenticationState(pessoa);
    }

    public async Task<AuthResponse> LoginAsync(string email, string senha)
    {
        var response = await _httpClient.PostAsJsonAsync("auth/login?useCookies=true", new
        {
            email,
            password = senha
        });

        if (response.IsSuccessStatusCode)
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return new AuthResponse { Sucesso = true };
        }

        return new AuthResponse { Sucesso = false, Erros = ["E-mail ou senha inválidos."] };
    }

    public async Task<AuthResponse> RegistrarAsync(string email, string senha)
    {
        var response = await _httpClient.PostAsJsonAsync("auth/register", new
        {
            email,
            password = senha
        });

        try
        {
            await response.GarantirSucessoAsync();
        }
        catch (HttpRequestException ex)
        {
            return new AuthResponse { Sucesso = false, Erros = [TraduzirErroIdentity(ex.Message)] };
        }

        return await LoginAsync(email, senha);
    }

    public async Task LogoutAsync()
    {
        await _httpClient.PostAsync("auth/logout", null);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>Recarrega nome, foto e papéis depois que o perfil é alterado.</summary>
    public void AtualizarEstado() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public async Task<bool> IsAuthenticated()
    {
        await GetAuthenticationStateAsync();
        return autenticado;
    }

    // O Identity responde em inglês; traduz as mensagens mais comuns do cadastro.
    private static string TraduzirErroIdentity(string mensagem)
    {
        var traducoes = new (string Trecho, string Traducao)[]
        {
            ("is already taken", "Este e-mail já está cadastrado."),
            ("must be at least", "A senha deve ter pelo menos 6 caracteres."),
            ("non alphanumeric", "A senha precisa de um caractere especial (ex.: !@#)."),
            ("at least one digit", "A senha precisa de pelo menos um número."),
            ("at least one uppercase", "A senha precisa de uma letra maiúscula."),
            ("at least one lowercase", "A senha precisa de uma letra minúscula."),
            ("is invalid", "E-mail inválido.")
        };

        var encontradas = traducoes
            .Where(t => mensagem.Contains(t.Trecho, StringComparison.OrdinalIgnoreCase))
            .Select(t => t.Traducao)
            .Distinct()
            .ToList();

        return encontradas.Count > 0 ? string.Join(" ", encontradas) : mensagem;
    }
}
