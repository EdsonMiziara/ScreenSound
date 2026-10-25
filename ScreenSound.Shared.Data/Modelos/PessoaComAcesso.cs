using Microsoft.AspNetCore.Identity;

namespace ScreenSound.Shared.Data.Modelos;

public class PessoaComAcesso : IdentityUser<int>
{
    public string? NomeExibicao { get; set; }
    public string? Bio { get; set; }
    public string? FotoPerfil { get; set; }

    /// <summary>Nome público: o nome de exibição ou, na falta dele, a parte do e-mail antes do @.</summary>
    public string NomePublico =>
        !string.IsNullOrWhiteSpace(NomeExibicao) ? NomeExibicao : (UserName ?? Email ?? "ouvinte").Split('@')[0];
}
