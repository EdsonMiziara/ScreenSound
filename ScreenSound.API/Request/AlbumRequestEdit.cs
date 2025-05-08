using ScreenSound.Modelos;

namespace ScreenSound.API.Request;

public record class AlbumRequestEdit(int Id, string Nome, ICollection<Musica> Musicas, int ArtistaId);
