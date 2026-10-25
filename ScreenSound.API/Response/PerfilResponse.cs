namespace ScreenSound.API.Response;

public record PerfilResponse(
    int Id,
    string Email,
    string? NomeExibicao,
    string NomePublico,
    string? Bio,
    string? FotoPerfil,
    ICollection<string> Papeis,
    bool ContaDemo);

public record UsuarioResponse(int Id, string Email, string NomePublico, string? FotoPerfil, bool Admin, bool ContaDemo);
