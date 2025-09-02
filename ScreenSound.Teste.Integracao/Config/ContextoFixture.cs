using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using Testcontainers.MsSql;

namespace ScreenSound.Teste.Integracao.Config;

public class ContextoFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private MsSqlContainer _dbContainer;
    private IServiceScope _scope;

    public ScreenSoundContext Context { get; private set; }

    public ContextoFixture()
    {
        _dbContainer = new MsSqlBuilder()
    .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
    .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.Test.json", optional: true);
            var inMemorySettings = new Dictionary<string, string>
            {
                {"ConnectionStrings:ScreenSoundDB", _dbContainer.GetConnectionString()}
            };
            config.AddInMemoryCollection(inMemorySettings);
            context.Configuration = config.Build(); // Reconstrui a configuração

            // Configurações de diretórios (sem logs)
            var contentRoot = Directory.GetCurrentDirectory();
            var wwwrootDir = Path.Combine(contentRoot, "wwwroot");
            if (!Directory.Exists(wwwrootDir)) { Directory.CreateDirectory(wwwrootDir); }
            var fotosPerfilDir = Path.Combine(wwwrootDir, "FotosPerfil");
            if (!Directory.Exists(fotosPerfilDir)) { Directory.CreateDirectory(fotosPerfilDir); }
        });

        builder.ConfigureServices(services =>
        {
            // Remove o registro do contexto de banco de dados da aplicação principal
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ScreenSoundContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Adiciona o DbContext usando a string de conexão do contêiner
            services.AddDbContext<ScreenSoundContext>((serviceProvider, options) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("ScreenSoundDB");
                options.UseSqlServer(connectionString);
            });

            // Configura o esquema de autenticação de teste
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(TestAuthHandler.SchemeName, options => { });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Atualiza a IConfiguration com a string de conexão final do contêiner.
        var newConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"ConnectionStrings:ScreenSoundDB", _dbContainer.GetConnectionString()}
            })
            .Build();

        this.WithWebHostBuilder(b => b.UseConfiguration(newConfig));

        // Força a reconstrução do provedor de serviços
        var client = CreateClient();

        _scope = Services.CreateScope();
        var scopedServices = _scope.ServiceProvider;
        Context = scopedServices.GetRequiredService<ScreenSoundContext>();

        const int maxRetries = 5;
        const int delayMs = 3000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await Context.Database.MigrateAsync();
                break;
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                if (ex.Message.Contains("Invalid object name") || ex.Message.Contains("Cannot open database") || ex.Message.Contains("Login failed"))
                {
                    await Task.Delay(delayMs);
                    if (i == maxRetries - 1)
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Popula os dados de teste APÓS as migrações (sem logs detalhados)
        await PopularDadosTeste();
    }

    private async Task PopularDadosTeste()
    {
        var artistaExistente = await Context.Artistas.FirstOrDefaultAsync(a => a.Nome == "Djavan");
        if (artistaExistente == null)
        {
            var artista = new Artista("Djavan", "Djavan Caetano Viana é um cantor, compositor, arranjador, produtor musical, empresário, violonista e ex-futebolista brasileiro.")
            {
                FotoPerfil = "https://cdn.pixabay.com/photo/2016/08/08/09/17/avatar-1577909_1280.png"
            };
            Context.Artistas.Add(artista);
            await Context.SaveChangesAsync();
        }
    }

    private async Task LimparBancoDeDados()
    {
        try
        {
            Context.Avaliacoes.RemoveRange(await Context.Avaliacoes.AsNoTracking().ToListAsync());
            Context.Musicas.RemoveRange(await Context.Musicas.AsNoTracking().ToListAsync());
            Context.Albuns.RemoveRange(await Context.Albuns.AsNoTracking().ToListAsync());
            Context.Generos.RemoveRange(await Context.Generos.AsNoTracking().ToListAsync());
            Context.Artistas.RemoveRange(await Context.Artistas.AsNoTracking().ToListAsync());
            await Context.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Ignora erros na limpeza, o foco é na migração e nos testes
        }
    }

    public override async ValueTask DisposeAsync()
    {
        if (_scope != null)
        {
            await LimparBancoDeDados(); // Limpa o banco de dados antes de descartar
            _scope.Dispose();
        }
        await _dbContainer.StopAsync();
        await base.DisposeAsync();

        // Adiciona a chamada para GC.SuppressFinalize para atender à regra CA1816
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Cria um HttpClient que simula um usuário autenticado usando o TestAuthHandler.
    /// </summary>
    public HttpClient CreateClientAuthorized()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // CORREÇÃO AQUI: Adicione o cabeçalho diretamente à propriedade DefaultRequestHeaders
        client.DefaultRequestHeaders.Add("Authorization", $"{TestAuthHandler.SchemeName} FakeToken");

        return client;
    }

    /// <summary>
    /// Cria um HttpClient que não simula autenticação.
    /// </summary>
    public HttpClient CreateClientUnauthorized()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
