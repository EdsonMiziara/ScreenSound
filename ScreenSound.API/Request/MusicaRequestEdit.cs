using ScreenSound.Modelos;

namespace ScreenSound.API.Requests;

public record MusicaRequestEdit(string NomeEdit, int ArtistaIdEdit, int AnoLancamentoEdit, ICollection<GeneroRequest>? GenerosEdit = null,int? IdEdit = null)
    : MusicaRequest(NomeEdit, ArtistaIdEdit, AnoLancamentoEdit, GenerosEdit);