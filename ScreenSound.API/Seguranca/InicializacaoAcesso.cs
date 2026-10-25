using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ScreenSound.Shared.Data.Modelos;

namespace ScreenSound.API.Seguranca;

public static class InicializacaoAcesso
{
    /// <summary>
    /// Garante que o perfil de administrador exista, promove os e-mails configurados
    /// e cria as contas de demonstração. Falhas são apenas registradas para não derrubar a API
    /// (ex.: banco ainda sem migrations aplicadas).
    /// </summary>
    public static async Task InicializarAcessoAsync(this WebApplication app)
    {
        using var escopo = app.Services.CreateScope();
        var servicos = escopo.ServiceProvider;
        var logger = servicos.GetRequiredService<ILogger<OpcoesAcesso>>();

        try
        {
            var opcoes = servicos.GetRequiredService<IOptions<OpcoesAcesso>>().Value;
            var perfis = servicos.GetRequiredService<RoleManager<PerfilDeAcesso>>();
            var usuarios = servicos.GetRequiredService<UserManager<PessoaComAcesso>>();

            if (!await perfis.RoleExistsAsync(Perfis.Administrador))
            {
                await perfis.CreateAsync(new PerfilDeAcesso { Name = Perfis.Administrador });
            }

            foreach (var conta in opcoes.ContasDemo)
            {
                var pessoa = await usuarios.FindByEmailAsync(conta.Email);
                if (pessoa is null)
                {
                    pessoa = new PessoaComAcesso { UserName = conta.Email, Email = conta.Email, NomeExibicao = conta.Nome };
                    var resultado = await usuarios.CreateAsync(pessoa, conta.Senha);
                    if (!resultado.Succeeded)
                    {
                        logger.LogWarning("Não foi possível criar a conta demo {Email}: {Erros}",
                            conta.Email, string.Join("; ", resultado.Errors.Select(e => e.Description)));
                        continue;
                    }
                }

                await GarantirPerfilAsync(usuarios, pessoa, conta.Admin);
            }

            foreach (var email in opcoes.Administradores)
            {
                var pessoa = await usuarios.FindByEmailAsync(email);
                if (pessoa is not null) await GarantirPerfilAsync(usuarios, pessoa, admin: true);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Inicialização de perfis e contas demo ignorada.");
        }
    }

    private static async Task GarantirPerfilAsync(UserManager<PessoaComAcesso> usuarios, PessoaComAcesso pessoa, bool admin)
    {
        var ehAdmin = await usuarios.IsInRoleAsync(pessoa, Perfis.Administrador);
        if (admin && !ehAdmin) await usuarios.AddToRoleAsync(pessoa, Perfis.Administrador);
        if (!admin && ehAdmin) await usuarios.RemoveFromRoleAsync(pessoa, Perfis.Administrador);
    }
}
