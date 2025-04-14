using ScreenSound.Modelos;

namespace ScreenSound.API.Requests;

public record MusicaRequestEdit(string NomeEdit, int? ArtistaIdEdit, int IdEdit, int? AnoLancamentoEdit)
    : MusicaRequest(NomeEdit, ArtistaIdEdit, IdEdit, AnoLancamentoEdit);