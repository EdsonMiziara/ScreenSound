using ScreenSound.Shared.Modelos.Auxiliar;
using System.Text.Json.Serialization;

namespace ScreenSound.Web.Response;

public record MusicaResponse(int Id, string Nome, int ArtistaId, string? NomeArtista, ICollection<GeneroResponse> Generos, string? SpotifyUrl = null)
{
    public int? AnoLancamento { get; init; }
    public double? Classificacao { get; init; }

    /// <summary>
    /// Indica se a faixa pode ser tocada no player (tem link válido do Spotify).
    /// </summary>
    [JsonIgnore]
    public bool Tocavel => SpotifyLink.ParaUri(SpotifyUrl) is not null;
}
