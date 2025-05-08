using ScreenSound.Modelos;

namespace ScreenSound.API.Response;

public record class AlbumResponse(int Id, string Nome, ICollection<Musica> Musicas, ArtistaResponse artista = null)
{
    public double Classificacao { get; set; }

};
