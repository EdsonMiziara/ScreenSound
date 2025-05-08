using ScreenSound.Modelos;
using ScreenSound.Web.Response;

namespace ScreenSound.API.Response;

public record MusicaResponse(int Id, string Nome, int ArtistaId, string? NomeArtista, ICollection<GeneroResponse> generos)
{
    public double? Classificacao { get; set; }

};