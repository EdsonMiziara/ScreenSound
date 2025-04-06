
namespace ScreenSound.Models;

internal class Artista /*: IAvaliavel*/
{
    //private List<Album> albuns = new();
    private List<Musica> musicas = new();
    //private List<Avaliacao> notas = new();


    public Artista(string nome, string bio)
    {
        Nome = nome;
        Bio = bio;
        FotoPerfil = "https://cdn.pixabay.com/photo/2016/08/08/09/17/avatar-1577909_1280.png";
    }
    //public double Media
    //{
    //    get
    //    {
    //        if (notas.Count == 0) return 0;
    //        else return notas.Average(a => a.Nota);
    //    }
    //}
    //public List<Album> Albuns => albuns;

    public string Nome { get; set; }
    public string FotoPerfil { get; set; }
    public string Bio { get; set; }
    public int Id { get; set; }

    //public void AdicionarAlbum(Album album)
    //{
    //    albuns.Add(album);
    //}

    //public void AdicionarNota(Avaliacao nota)
    //{
    //    notas.Add(nota);
    //}
    public void AdicionarMusica(Musica musica)
    {
        musicas.Add(musica);
    }
    public void ExibirDiscografia()
    {
        Console.WriteLine($"Discografia da banda {Nome}");
        foreach (var musica in musicas) { 
            Console.WriteLine($"Musicas: {musica.Nome})");
        }
    }

    public override string ToString()
    {
        return $"Id: {Id}\nNome da banda: {Nome}\nBio da Banda: {Bio}";
    }

}