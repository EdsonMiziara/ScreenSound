namespace ScreenSound.Web.Componentes;

/// <summary>Monta as URLs das páginas de detalhe (com escape dos nomes).</summary>
public static class Rotas
{
    public static string Musica(string nome) => $"/Musica/{Uri.EscapeDataString(nome)}";
    public static string EditarMusica(string nome) => $"/EditarMusica/{Uri.EscapeDataString(nome)}";
    public static string Artista(string? nome) => string.IsNullOrWhiteSpace(nome) ? "/Artistas" : $"/Artista/{Uri.EscapeDataString(nome)}";
    public static string EditarArtista(string nome) => $"/EditarArtista/{Uri.EscapeDataString(nome)}";
    public static string Playlist(int id) => $"/Playlist/{id}";
    public static string Genero(string genero) => $"/Musicas?genero={Uri.EscapeDataString(genero)}";
    public static string Busca(string termo) => $"/Buscar?q={Uri.EscapeDataString(termo)}";
}
