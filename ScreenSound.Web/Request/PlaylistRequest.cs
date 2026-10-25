namespace ScreenSound.Web.Requests;

public record PlaylistRequest(string Nome, string? Descricao, bool Publica = true, ICollection<int>? MusicaIds = null);

public record PlaylistOrdemRequest(ICollection<int> MusicaIds);
