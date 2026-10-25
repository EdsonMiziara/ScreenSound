namespace ScreenSound.API.Response;

public record PlaylistResponse(
    int Id,
    string Nome,
    string? Descricao,
    bool Publica,
    bool EhDono,
    string? NomeDono,
    DateTime CriadaEm,
    ICollection<MusicaResponse> Musicas)
{
    public int TotalMusicas => Musicas.Count;
};
