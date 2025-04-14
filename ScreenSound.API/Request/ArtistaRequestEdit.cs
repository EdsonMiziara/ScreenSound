namespace ScreenSound.API.Request;

public record ArtistaRequestEdit(int id, string nome, string bio) : ArtistaRequest(nome, bio);