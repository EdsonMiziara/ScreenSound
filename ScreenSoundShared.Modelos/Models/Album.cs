using System.Security.Principal;

namespace ScreenSound.Models;
public class Album : IAvaliavel
{
    public virtual ICollection<Musica> Musicas { get; set; } = new List<Musica>();
    public virtual ICollection<Avaliacao> Notas { get; set; } = new List<Avaliacao>();
    public virtual Artista? Artista { get; set; }
    public Album(string nome)
    {
        Nome = nome;
    }

    public string Nome { get; set; }
    public int DuracaoTotal => Musicas.Sum(m => m.Duracao);
    public int Id { get; set; }

    public double Media
    {
        get
        {
            if (Notas.Count == 0) return 0;
            else return Notas.Average(a => a.Nota);
        }
    }

    public void AdicionarMusica(Musica musica)
    {
        Musicas.Add(musica);
    }

    public void AdicionarNota(Avaliacao nota)
    {
        Notas.Add(nota);
    }

    public void ExibirMusicasDoAlbum()
    {
        Console.WriteLine($"Lista de músicas do álbum {Nome}:\n");
        foreach (var musica in Musicas)
        {
            Console.WriteLine($"Música: {musica.Nome}");
        }
        Console.WriteLine($"\nPara ouvir este álbum inteiro você precisa de {DuracaoTotal}");
    }
}