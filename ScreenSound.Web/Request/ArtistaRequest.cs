using System.ComponentModel.DataAnnotations;

namespace ScreenSound.Web.Request;

public record ArtistaRequest([Required] string nome, [Required] string bio);
