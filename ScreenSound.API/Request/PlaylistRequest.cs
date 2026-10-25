using System.ComponentModel.DataAnnotations;

namespace ScreenSound.API.Requests;

public record PlaylistRequest([Required] string Nome, string? Descricao, bool Publica = true, ICollection<int>? MusicaIds = null);

public record PlaylistOrdemRequest(ICollection<int> MusicaIds);
