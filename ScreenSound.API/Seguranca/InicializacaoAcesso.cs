using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ScreenSound.Shared.Data.Modelos;

namespace ScreenSound.API.Seguranca;

public static class InicializacaoAcesso
{
    private const int MaximoTentativas = 30;
    private static readonly TimeSpan IntervaloEntreTentativas = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Agenda, para depois que a API subir, a criação do perfil de administrador, a promoção dos
    /// e-mails configurados e a criação das contas demo. Roda em segundo plano e tenta de novo
    /// enquanto o banco estiver indisponível (ex.: Azure SQL serverless "acordando" da pausa),
    /// sem atrasar a inicialização da API.
    /// </summary>
    /// <param name="aoConcluir">Executado depois que as contas existirem (ex.: catálogo demo, que usa a conta admin).</param>
    public static void AgendarInicializacaoAcesso(this WebApplication app, Func<Task>? aoConcluir = null)
    {
        app.Lifetime.ApplicationStarted.Register(() =>
            _ = Task.Run(() => InicializarComRetentativasAsync(app.Services, app.Lifetime.ApplicationStopping, aoConcluir)));
    }

    private static async Task InicializarComRetentativasAsync(IServiceProvider servicos, CancellationToken parada, Func<Task>? aoConcluir)
    {
        var logger = servicos.GetRequiredService<ILogger<OpcoesAcesso>>();

        for (var tentativa = 1; tentativa <= MaximoTentativas; tentativa++)
        {
            if (parada.IsCancellationRequested) return;

            try
            {
                await InicializarAsync(servicos, logger);
                logger.LogInformation("Perfis e contas demo inicializados (tentativa {Tentativa}).", tentativa);

                if (aoConcluir is not null) await aoConcluir();
                return;
            }
            catch (Exception ex) when (!parada.IsCancellationRequested)
            {
                logger.LogWarning("Inicialização de perfis e contas demo falhou (tentativa {Tentativa}/{Maximo}): {Erro}. Nova tentativa em {Segundos}s.",
                    tentativa, MaximoTentativas, ex.GetBaseException().Message, IntervaloEntreTentativas.TotalSeconds);
            }
            catch (Exception)
            {
                return; // API encerrando
            }

            try
            {
                await Task.Delay(IntervaloEntreTentativas, parada);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        logger.LogError("Não foi possível inicializar perfis e contas demo após {Maximo} tentativas. Reinicie a API com o banco disponível.", MaximoTentativas);
    }

    private static async Task InicializarAsync(IServiceProvider raiz, ILogger logger)
    {
        using var escopo = raiz.CreateScope();
        var servicos = escopo.ServiceProvider;
        var opcoes = servicos.GetRequiredService<IOptions<OpcoesAcesso>>().Value;
        var perfis = servicos.GetRequiredService<RoleManager<PerfilDeAcesso>>();
        var usuarios = servicos.GetRequiredService<UserManager<PessoaComAcesso>>();

        if (!await perfis.RoleExistsAsync(Perfis.Administrador))
        {
            Garantir(await perfis.CreateAsync(new PerfilDeAcesso { Name = Perfis.Administrador }), "criar o perfil de administrador");
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
                    // Erro de dados (ex.: senha fora da política): tentar de novo não resolve, segue com as demais contas
                    logger.LogWarning("Não foi possível criar a conta demo {Email}: {Erros}", conta.Email, Erros(resultado));
                    continue;
                }
            }

            await GarantirPerfilAsync(usuarios, pessoa, conta.Admin);
        }

        foreach (var email in opcoes.Administradores)
        {
            var pessoa = await usuarios.FindByEmailAsync(email);
            if (pessoa is not null) await GarantirPerfilAsync(usuarios, pessoa, admin: true);
            else logger.LogWarning("Administrador configurado {Email} ainda não tem conta cadastrada.", email);
        }
    }

    private static async Task GarantirPerfilAsync(UserManager<PessoaComAcesso> usuarios, PessoaComAcesso pessoa, bool admin)
    {
        var ehAdmin = await usuarios.IsInRoleAsync(pessoa, Perfis.Administrador);
        if (admin && !ehAdmin) Garantir(await usuarios.AddToRoleAsync(pessoa, Perfis.Administrador), $"promover {pessoa.Email} a administrador");
        if (!admin && ehAdmin) Garantir(await usuarios.RemoveFromRoleAsync(pessoa, Perfis.Administrador), $"remover o perfil de administrador de {pessoa.Email}");
    }

    private static void Garantir(IdentityResult resultado, string acao)
    {
        if (!resultado.Succeeded) throw new InvalidOperationException($"Falha ao {acao}: {Erros(resultado)}");
    }

    private static string Erros(IdentityResult resultado) => string.Join("; ", resultado.Errors.Select(e => e.Description));
}
