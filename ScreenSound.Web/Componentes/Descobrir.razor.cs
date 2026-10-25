using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using ScreenSound.Web.Componentes.Dialogos;
using ScreenSound.Web.Response;

namespace ScreenSound.Web.Componentes;

// Código da página "Descobrir" em code-behind: o parser Razor do SDK 9 (usado no CI e no Docker)
// não interpreta bem padrões relacionais de switch (ex.: "< 12 =>") dentro de @code.
public partial class Descobrir
{

    [CascadingParameter] private Task<AuthenticationState>? AuthState { get; set; }

    private string nome = "ouvinte";
    private List<MusicaResponse>? musicas;
    private List<MusicaResponse>? novidades;
    private List<MusicaResponse>? emAlta;
    private List<ArtistaResponse>? artistas;
    private List<PlaylistResponse>? minhas;
    private List<PlaylistResponse>? comunidade;
    private int totalArtistas;
    private int totalGeneros;

    private static string Saudacao => DateTime.Now.Hour switch
    {
        < 5 => "Boa noite",
        < 12 => "Bom dia",
        < 18 => "Boa tarde",
        _ => "Boa noite"
    };

    protected override async Task OnInitializedAsync()
    {
        Player.OnMudanca += AoMudar;

        if (AuthState is not null)
        {
            var estado = await AuthState;
            nome = estado.User.FindFirst(ClaimsScreenSound.Nome)?.Value ?? nome;
        }

        var tarefaMusicas = MusicaAPI.GetMusicasAsync();
        var tarefaArtistas = ArtistaAPI.GetArtistasAsync();
        var tarefaMinhas = CarregarSemFalharAsync(PlaylistAPI.GetMinhasAsync());
        var tarefaPublicas = CarregarSemFalharAsync(PlaylistAPI.GetPublicasAsync());

        musicas = (await tarefaMusicas).OrderByDescending(m => m.Id).ToList();
        novidades = musicas.Take(12).ToList();
        emAlta = musicas
            .Where(m => m.Classificacao > 0)
            .OrderByDescending(m => m.Classificacao)
            .ThenByDescending(m => m.Tocavel)
            .Take(4)
            .ToList();
        totalGeneros = musicas.SelectMany(m => m.Generos).Select(g => g.Nome.ToLowerInvariant()).Distinct().Count();

        var todosArtistas = (await tarefaArtistas)?.ToList() ?? new();
        totalArtistas = todosArtistas.Count;
        artistas = todosArtistas
            .OrderByDescending(a => a.Classificacao ?? 0)
            .ThenByDescending(a => FaixasDo(a.Id))
            .Take(10)
            .ToList();

        minhas = await tarefaMinhas;
        comunidade = (await tarefaPublicas).Where(p => !p.EhDono && p.TotalMusicas > 0).Take(10).ToList();
    }

    private int FaixasDo(int artistaId) => musicas?.Count(m => m.ArtistaId == artistaId) ?? 0;

    // Recebe a tarefa já iniciada: o Razor do SDK 9 não interpreta bem delegates com genéricos aninhados
    private static async Task<List<PlaylistResponse>> CarregarSemFalharAsync(Task<List<PlaylistResponse>> carregamento)
    {
        try
        {
            return await carregamento;
        }
        catch (HttpRequestException)
        {
            return new();
        }
    }

    private async Task TocarAleatorio()
    {
        if (musicas is null || !await Player.TocarListaAsync(musicas, embaralhar: true))
        {
            Snackbar.Add("Nenhuma faixa com link do Spotify para tocar ainda.", Severity.Info);
        }
    }

    private async Task NovaPlaylist()
    {
        var id = await DialogService.FormularioPlaylistAsync();
        if (id is int novo) Navigation.NavigateTo(Rotas.Playlist(novo));
    }

    private void AoMudar() => InvokeAsync(StateHasChanged);

    public void Dispose() => Player.OnMudanca -= AoMudar;
}
