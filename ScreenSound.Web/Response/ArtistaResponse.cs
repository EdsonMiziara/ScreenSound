using System.Text.Json.Serialization;

namespace ScreenSound.Web.Response;

public record ArtistaResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("bio")] string Bio,
    [property: JsonPropertyName("fotoPerfil")] string? FotoPerfil
)
{
    public override string ToString()
    {
        return $"{Nome}";
    }

    public double? Classificacao { get; set; }
}