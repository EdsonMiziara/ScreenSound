using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ScreenSound.Shared.Modelos.Auxiliar;
using ScreenSound.Web.Response;
using System.Text.Json;

namespace ScreenSound.Web.Services;

/// <summary>
/// Player global (barra inferior). Mantém a fila de reprodução e conversa com o
/// iFrame API do Spotify via JS interop (wwwroot/js/screensound.js).
/// </summary>
public sealed class PlayerService : IAsyncDisposable
{
    public const string HostId = "ss-embed-host";
    private const string ChaveHistorico = "ss-historico";
    private const string ChaveVolume = "ss-volume";
    private const int TamanhoHistorico = 12;

    private readonly IJSRuntime js;
    private readonly IConfiguration configuracao;
    private readonly NavigationManager navegacao;
    private bool spotifyJsConfigurado;
    private DotNetObjectReference<PlayerService>? referencia;
    private List<MusicaResponse> fila = new();
    private List<MusicaResponse> historico = new();
    private bool historicoCarregado;

    public PlayerService(IJSRuntime js, IConfiguration configuracao, NavigationManager navegacao)
    {
        this.js = js;
        this.configuracao = configuracao;
        this.navegacao = navegacao;
    }

    public IReadOnlyList<MusicaResponse> Fila => fila;
    public IReadOnlyList<MusicaResponse> Historico => historico;
    public int Indice { get; private set; } = -1;
    public MusicaResponse? Atual => Indice >= 0 && Indice < fila.Count ? fila[Indice] : null;
    public bool Tocando { get; private set; }
    public bool Carregando { get; private set; }
    public double Posicao { get; private set; }
    public double Duracao { get; private set; }
    public double Progresso => Duracao > 0 ? Math.Clamp(Posicao / Duracao, 0, 1) : 0;
    public bool TemProxima => Indice < fila.Count - 1;
    public bool TemAnterior => Indice > 0;

    /// <summary>ClientId do Spotify configurado: habilita o botão "Conectar Spotify Premium".</summary>
    public bool SpotifyConfigurado { get; private set; }

    /// <summary>Tocando pelo Web Playback SDK (Premium), com controle de volume.</summary>
    public bool Premium { get; private set; }
    public bool ConectandoPremium { get; private set; }
    public string? ContaSpotify { get; private set; }
    public string? ErroPremium { get; private set; }
    public double Volume { get; private set; } = 0.7;
    public bool Mudo { get; private set; }

    /// <summary>Troca de faixa, play/pause, fila.</summary>
    public event Action? OnMudanca;

    /// <summary>Atualização de posição (várias vezes por segundo).</summary>
    public event Action? OnProgresso;

    public bool EstaTocando(int musicaId) => Atual?.Id == musicaId && (Tocando || Carregando);

    /// <summary>
    /// Toca a faixa usando a lista de contexto como fila (ex.: a playlist ou a página atual).
    /// Se a faixa já é a atual, alterna play/pause. Retorna false se a faixa não tiver link do Spotify.
    /// </summary>
    public async Task<bool> TocarAsync(MusicaResponse musica, IEnumerable<MusicaResponse>? contexto = null)
    {
        if (!musica.Tocavel) return false;

        if (Atual?.Id == musica.Id)
        {
            await AlternarAsync();
            return true;
        }

        var novaFila = (contexto ?? new[] { musica }).Where(m => m.Tocavel).DistinctBy(m => m.Id).ToList();
        var indice = novaFila.FindIndex(m => m.Id == musica.Id);
        if (indice < 0)
        {
            novaFila.Insert(0, musica);
            indice = 0;
        }

        fila = novaFila;
        Indice = indice;
        await CarregarAtualAsync();
        return true;
    }

    /// <summary>Toca uma lista inteira desde o começo. Retorna false se nenhuma faixa for tocável.</summary>
    public async Task<bool> TocarListaAsync(IEnumerable<MusicaResponse> musicas, bool embaralhar = false)
    {
        var tocaveis = musicas.Where(m => m.Tocavel).DistinctBy(m => m.Id).ToList();
        if (tocaveis.Count == 0) return false;

        if (embaralhar)
        {
            tocaveis = tocaveis.OrderBy(_ => Random.Shared.Next()).ToList();
        }

        fila = tocaveis;
        Indice = 0;
        await CarregarAtualAsync();
        return true;
    }

    public void AdicionarNaFila(MusicaResponse musica)
    {
        if (!musica.Tocavel || fila.Any(m => m.Id == musica.Id)) return;

        fila.Add(musica);
        OnMudanca?.Invoke();
    }

    public async Task AlternarAsync()
    {
        if (Atual is null) return;
        await js.InvokeVoidAsync("ssPlayer.toggle");
    }

    public async Task ProximaAsync()
    {
        if (!TemProxima) return;
        Indice++;
        await CarregarAtualAsync();
    }

    public async Task AnteriorAsync()
    {
        // Comportamento padrão de players: se já passou do começo, volta para o início da faixa
        if (Posicao > 3000 || !TemAnterior)
        {
            await BuscarAsync(0);
            return;
        }

        Indice--;
        await CarregarAtualAsync();
    }

    public async Task IrParaAsync(int indice)
    {
        if (indice < 0 || indice >= fila.Count || indice == Indice) return;
        Indice = indice;
        await CarregarAtualAsync();
    }

    public async Task BuscarAsync(double fracao)
    {
        if (Atual is null || Duracao <= 0) return;
        var segundos = Math.Clamp(fracao, 0, 1) * Duracao / 1000;
        await js.InvokeVoidAsync("ssPlayer.seek", segundos);
    }

    public async Task FecharAsync()
    {
        await js.InvokeVoidAsync("ssPlayer.destroy");
        fila = new();
        Indice = -1;
        Tocando = false;
        Carregando = false;
        Posicao = Duracao = 0;
        OnMudanca?.Invoke();
    }

    /// <summary>
    /// Configura o login do Spotify e, se a pessoa já conectou antes, reativa o modo Premium.
    /// </summary>
    public async Task InicializarSpotifyAsync()
    {
        try
        {
            await PrepararSpotifyAsync();

            if (double.TryParse(await js.InvokeAsync<string?>("ssStorage.get", ChaveVolume),
                    System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var volume))
            {
                Volume = Math.Clamp(volume, 0, 1);
            }

            if (SpotifyConfigurado && !Premium && !ConectandoPremium && await js.InvokeAsync<bool>("ssSpotify.conectado"))
            {
                await AtivarPremiumAsync();
            }
        }
        catch (JSException)
        {
            SpotifyConfigurado = false;
        }

        OnMudanca?.Invoke();
    }

    /// <summary>
    /// Envia ClientId e URL de retorno ao JS (idempotente). Spotify:RedirectUri é opcional;
    /// por padrão usa {base}/spotify/callback, que precisa estar cadastrada no Spotify Developer.
    /// </summary>
    public async Task PrepararSpotifyAsync()
    {
        if (spotifyJsConfigurado) return;
        spotifyJsConfigurado = true;

        var clientId = configuracao["Spotify:ClientId"];
        var redirectUri = configuracao["Spotify:RedirectUri"];
        if (string.IsNullOrWhiteSpace(redirectUri)) redirectUri = navegacao.BaseUri + "spotify/callback";

        SpotifyConfigurado = !string.IsNullOrWhiteSpace(clientId);
        await js.InvokeVoidAsync("ssSpotify.configurar", clientId, redirectUri);
    }

    /// <summary>Redireciona para o login do Spotify; ao voltar, a página de callback ativa o modo Premium.</summary>
    public async Task ConectarSpotifyAsync(string retorno)
    {
        await PrepararSpotifyAsync();
        if (!SpotifyConfigurado) return;
        await js.InvokeVoidAsync("ssSpotify.iniciarLogin", retorno);
    }

    public async Task AtivarPremiumAsync()
    {
        await PrepararSpotifyAsync();
        ConectandoPremium = true;
        ErroPremium = null;
        OnMudanca?.Invoke();

        referencia ??= DotNetObjectReference.Create(this);
        await js.InvokeVoidAsync("ssPlayer.ativarPremium", referencia, Mudo ? 0 : Volume);
    }

    public async Task DesconectarSpotifyAsync()
    {
        await js.InvokeVoidAsync("ssPlayer.desativarPremium");
        await js.InvokeVoidAsync("ssSpotify.desconectar");
        Premium = false;
        ContaSpotify = null;
        OnMudanca?.Invoke();

        if (Atual is not null) await CarregarAtualAsync();
    }

    public async Task DefinirVolumeAsync(double volume)
    {
        Volume = Math.Clamp(volume, 0, 1);
        Mudo = Volume == 0;
        await js.InvokeVoidAsync("ssPlayer.setVolume", Volume);
        await js.InvokeVoidAsync("ssStorage.set", ChaveVolume, Volume.ToString(System.Globalization.CultureInfo.InvariantCulture));
        OnMudanca?.Invoke();
    }

    public async Task AlternarMudoAsync()
    {
        Mudo = !Mudo;
        if (!Mudo && Volume == 0) Volume = 0.5;
        await js.InvokeVoidAsync("ssPlayer.setVolume", Mudo ? 0 : Volume);
        OnMudanca?.Invoke();
    }

    [JSInvokable]
    public async Task AtualizarModo(bool premium, string? conta, string? erro)
    {
        var mudou = Premium != premium;
        Premium = premium;
        ContaSpotify = conta;
        ErroPremium = erro;
        ConectandoPremium = false;
        OnMudanca?.Invoke();

        // Troca de motor no meio da reprodução: recarrega a faixa atual no novo player
        if (mudou && Atual is not null) await CarregarAtualAsync();
    }

    public async Task CarregarHistoricoAsync()
    {
        if (historicoCarregado) return;
        historicoCarregado = true;

        try
        {
            var json = await js.InvokeAsync<string?>("ssStorage.get", ChaveHistorico);
            if (!string.IsNullOrWhiteSpace(json))
            {
                historico = JsonSerializer.Deserialize<List<MusicaResponse>>(json) ?? new();
                OnMudanca?.Invoke();
            }
        }
        catch (Exception)
        {
            historico = new();
        }
    }

    [JSInvokable]
    public void AtualizarReproducao(bool pausado, double posicao, double duracao, bool carregando)
    {
        var mudouEstado = Tocando == pausado || Carregando != carregando;

        Tocando = !pausado;
        Carregando = carregando && pausado;
        Posicao = posicao;
        Duracao = duracao;

        if (mudouEstado) OnMudanca?.Invoke();
        OnProgresso?.Invoke();
    }

    [JSInvokable]
    public async Task FaixaTerminou()
    {
        if (TemProxima)
        {
            await ProximaAsync();
            return;
        }

        Tocando = false;
        OnMudanca?.Invoke();
    }

    private async Task CarregarAtualAsync()
    {
        var atual = Atual;
        var uri = SpotifyLink.ParaUri(atual?.SpotifyUrl);
        if (atual is null || uri is null) return;

        Posicao = 0;
        Duracao = 0;
        Tocando = false;
        Carregando = true;
        await RegistrarHistoricoAsync(atual);
        OnMudanca?.Invoke();
        OnProgresso?.Invoke();

        referencia ??= DotNetObjectReference.Create(this);
        await js.InvokeVoidAsync("ssPlayer.load", HostId, uri, referencia);
    }

    private async Task RegistrarHistoricoAsync(MusicaResponse musica)
    {
        historico.RemoveAll(m => m.Id == musica.Id);
        historico.Insert(0, musica);
        if (historico.Count > TamanhoHistorico)
        {
            historico.RemoveRange(TamanhoHistorico, historico.Count - TamanhoHistorico);
        }

        try
        {
            await js.InvokeVoidAsync("ssStorage.set", ChaveHistorico, JsonSerializer.Serialize(historico));
        }
        catch (Exception)
        {
            // localStorage pode estar indisponível (aba anônima); o histórico continua em memória
        }
    }

    public ValueTask DisposeAsync()
    {
        referencia?.Dispose();
        return ValueTask.CompletedTask;
    }
}
