using ScreenSound.Modelos;

namespace ScreenSound.API.Requests;

public record MusicaRequestEdit(string nome, int ArtistaId, int anoLancamento, ICollection<GeneroRequest>? Generos, int? Id)
    : MusicaRequest(nome, ArtistaId, anoLancamento, Generos);
