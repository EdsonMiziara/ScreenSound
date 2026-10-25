namespace ScreenSound.Shared.Modelos.Auxiliar;

public static class SpotifyLink
{
    private static readonly string[] TiposPermitidos = { "track", "album", "playlist", "artist", "episode", "show" };

    public static bool EhValido(string? link) => string.IsNullOrWhiteSpace(link) || Extrair(link) is not null;

    public static string? ParaEmbed(string? link) =>
        Extrair(link) is var (tipo, id) ? $"https://open.spotify.com/embed/{tipo}/{id}" : null;

    /// <summary>
    /// Converte o link no formato de URI do Spotify (spotify:track:id), usado pelo iFrame API.
    /// </summary>
    public static string? ParaUri(string? link) =>
        Extrair(link) is var (tipo, id) ? $"spotify:{tipo}:{id}" : null;

    /// <summary>
    /// Link canônico (sem query string / locale), usado para consultar o oEmbed do Spotify.
    /// </summary>
    public static string? ParaCanonico(string? link) =>
        Extrair(link) is var (tipo, id) ? $"https://open.spotify.com/{tipo}/{id}" : null;

    private static (string Tipo, string Id)? Extrair(string? link)
    {
        if (!Uri.TryCreate(link, UriKind.Absolute, out var uri)) return null;
        if (uri.Scheme != Uri.UriSchemeHttps || uri.Host != "open.spotify.com") return null;

        var partes = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length < 2) return null;
        var tipo = partes[^2];
        var id = partes[^1];
        if (!TiposPermitidos.Contains(tipo) || !id.All(char.IsLetterOrDigit)) return null;
        return (tipo, id);
    }
}
