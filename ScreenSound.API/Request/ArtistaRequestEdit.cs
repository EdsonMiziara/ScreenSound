namespace ScreenSound.API.Request;

public record ArtistaRequestEdit(int? id, string nome, string bio, string? fotoPerfil) : ArtistaRequest(nome, bio, fotoPerfil);