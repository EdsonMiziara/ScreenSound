
using ScreenSound.Modelos;
using ScreenSound.Web.Response;

namespace ScreenSound.Web.Requests;

public record MusicaRequestEdit(string nome, int ArtistaId, int AnoLancamento, ICollection<GeneroRequest>? Generos, int? Id, string? SpotifyUrl)
    : MusicaRequest(nome, ArtistaId, AnoLancamento, Generos, SpotifyUrl);
