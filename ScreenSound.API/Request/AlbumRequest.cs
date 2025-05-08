using ScreenSound.API.Requests;
using ScreenSound.Modelos;

namespace ScreenSound.API.Request;

public record class AlbumRequest(string Nome, int ArtistaId, ICollection<MusicaRequest>? Musicas = null);
