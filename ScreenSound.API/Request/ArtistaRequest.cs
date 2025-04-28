using System.ComponentModel.DataAnnotations;

namespace ScreenSound.API.Request;

public record ArtistaRequest([Required] string nome, [Required] string bio, string? fotoPerfil);
