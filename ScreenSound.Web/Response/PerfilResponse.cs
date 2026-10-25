namespace ScreenSound.Web.Response;

public record PerfilResponse(
    int Id,
    string Email,
    string? NomeExibicao,
    string NomePublico,
    string? Bio,
    string? FotoPerfil,
    ICollection<string> Papeis,
    bool ContaDemo)
{
    public bool Admin => Papeis.Contains(Perfis.Administrador);
}

public record UsuarioResponse(int Id, string Email, string NomePublico, string? FotoPerfil, bool Admin, bool ContaDemo);

public static class Perfis
{
    public const string Administrador = "Administrador";
}

/// <summary>Claims extras montadas a partir do perfil (ver AuthAPI).</summary>
public static class ClaimsScreenSound
{
    public const string Nome = "ss:nome";
    public const string Foto = "ss:foto";
    public const string Demo = "ss:demo";
}
