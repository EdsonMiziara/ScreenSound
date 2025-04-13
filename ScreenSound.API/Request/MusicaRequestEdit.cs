using ScreenSound.API.Requests;

namespace ScreenSound.API.Request;

public record MusicaRequestEdit(string nome, int artistaId, int id, int anoLancamento) : MusicaRequest(nome, artistaId, id, anoLancamento);
