using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ScreenSound.Teste.Integracao.Config;

// Opções para o nosso manipulador de autenticação de teste
public class TestAuthHandlerOptions : AuthenticationSchemeOptions
{
    public string DefaultUserId { get; set; } = "testuser";
    public string DefaultUserName { get; set; } = "Test User";
    public string DefaultRole { get; set; } = "Admin"; // Role padrão para o usuário de teste
}

// Manipulador de autenticação que intercepta requisições e as marca como autenticadas
public class TestAuthHandler : AuthenticationHandler<TestAuthHandlerOptions>
{
    // Nome do esquema de autenticação para referência
    public const string SchemeName = "TestScheme";

    public TestAuthHandler(
        IOptionsMonitor<TestAuthHandlerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Cria as claims (informações) para o usuário de teste
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Options.DefaultUserId),
            new Claim(ClaimTypes.Name, Options.DefaultUserName),
            new Claim(ClaimTypes.Role, Options.DefaultRole)
        };

        // Cria uma identidade e um principal de segurança com as claims
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        // Retorna um resultado de autenticação bem-sucedido
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}