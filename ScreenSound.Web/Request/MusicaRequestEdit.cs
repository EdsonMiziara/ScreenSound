
using ScreenSound.Modelos;
using ScreenSound.Web.Response;

namespace ScreenSound.Web.Requests;

public record MusicaRequestEdit(string nome, int ArtistaId, int anoLancamento, ICollection<GeneroRequest>? Generos, int? Id)
    : MusicaRequest(nome, ArtistaId, anoLancamento, Generos);
