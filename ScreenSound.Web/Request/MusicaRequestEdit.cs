
namespace ScreenSound.Web.Requests;

public record MusicaRequestEdit(string NomeEdit, int ArtistaIdEdit, int AnoLancamentoEdit, ICollection<GeneroRequest>? GenerosEdit = null,int? IdEdit = null)
    : MusicaRequest(NomeEdit, ArtistaIdEdit, AnoLancamentoEdit, GenerosEdit);