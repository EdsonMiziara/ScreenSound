using ScreenSound.Web.Services;

namespace ScreenSound.Web.Response;

public record MusicaResponse(int Id, string Nome, int anoLancamento, int ArtistaId, ICollection<GeneroResponse> Generos)
{
    public string ArtistaNome { get; set; } = string.Empty;
}