namespace ScreenSound.Web.Response;

public record class GeneroResponse (string Nome, string? DescricaoGenero, int? Id = null)
{
    public override string ToString()
    {
        return $"{Nome}";
    }
}
