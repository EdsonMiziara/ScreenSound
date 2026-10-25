using MudBlazor;
using ScreenSound.Web.Response;

namespace ScreenSound.Web.Componentes.Dialogos;

public static class Dialogos
{
    private static readonly DialogOptions Opcoes = new()
    {
        MaxWidth = MaxWidth.ExtraSmall,
        FullWidth = true,
        CloseButton = true,
        CloseOnEscapeKey = true
    };

    public static async Task<bool> ConfirmarAsync(this IDialogService dialogos, string titulo, string mensagem, string textoConfirmar = "Excluir")
    {
        var parametros = new DialogParameters<ConfirmarDialog>
        {
            { d => d.Mensagem, mensagem },
            { d => d.TextoConfirmar, textoConfirmar }
        };

        var referencia = await dialogos.ShowAsync<ConfirmarDialog>(titulo, parametros, Opcoes);
        var resultado = await referencia.Result;
        return resultado is { Canceled: false };
    }

    public static async Task AdicionarAPlaylistAsync(this IDialogService dialogos, MusicaResponse musica)
    {
        var parametros = new DialogParameters<AdicionarPlaylistDialog> { { d => d.Musica, musica } };
        var referencia = await dialogos.ShowAsync<AdicionarPlaylistDialog>("Adicionar à playlist", parametros, Opcoes);
        await referencia.Result;
    }

    /// <summary>Abre o formulário de criação/edição. Retorna o id da playlist salva, ou null se cancelado.</summary>
    public static async Task<int?> FormularioPlaylistAsync(this IDialogService dialogos, PlaylistResponse? playlist = null, MusicaResponse? musicaInicial = null)
    {
        var parametros = new DialogParameters<PlaylistFormDialog>
        {
            { d => d.Playlist, playlist },
            { d => d.MusicaInicial, musicaInicial }
        };

        var titulo = playlist is null ? "Nova playlist" : "Editar playlist";
        var referencia = await dialogos.ShowAsync<PlaylistFormDialog>(titulo, parametros, Opcoes);
        var resultado = await referencia.Result;
        return resultado is { Canceled: false, Data: int id } ? id : null;
    }
}
