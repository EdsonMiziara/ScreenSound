using ScreenSound.Shared.Modelos.Avaliacao;
using ScreenSound.Shared.Modelos.Validacao;

namespace ScreenSound.Modelos;

public class Artista : Valida, IAvaliavel<Artista>
{
    public virtual ICollection<Album> Albuns { get; set; } = new List<Album>();
    public virtual ICollection<Musica> Musicas { get; set; } = new List<Musica>();
    public virtual ICollection<Avaliacao> Notas { get; set; } = new List<Avaliacao>();


    public Artista(string nome, string bio)
    {
        Nome = nome;
        Bio = bio;
        FotoPerfil = "https://cdn.pixabay.com/photo/2016/08/08/09/17/avatar-1577909_1280.png";
        Validar();
    }
    public double Media
    {
        get
        {
            if (Notas.Count == 0) return 0;
            else return Notas.Average(a => a.Nota);
        }
    }
    public string Nome { get; set; }
    public string FotoPerfil { get; set; }
    public string Bio { get; set; }
    public int Id { get; set; }

    public void AdicionarAlbum(Album album)
    {
        if (album.EhValido) 
        {            
            Albuns.Add(album);
        }
        else
        {
            Console.WriteLine(album.Erros.Sumario); 
        }
    }

    public void AdicionarNota(int pessoaId, int nota)
    {
        nota = Math.Min(Math.Max(nota, 1), 5);

        Notas.Add(new Avaliacao(nota) { PessoaId = pessoaId });
    }
    public void AdicionarMusica(Musica musica)
    {
        Musicas.Add(musica);
    }
    public void ExibirDiscografia()
    {
        Console.WriteLine($"Discografia da banda {Nome}");
        foreach (var musica in Musicas)
        {
            Console.WriteLine($"Musicas: {musica.Nome})");
        }
    }

    public override string ToString()
    {
        return $"Id: {Id}\nNome da banda: {Nome}\nBio da Banda: {Bio}";
    }
    protected override void Validar()
    {
        if (Nome is null || Nome == "")
        {
            Erros.RegistrarErro("Nome não pode ser nulo ou vazio");
        }
        else if (Bio is null )
        {
            Erros.RegistrarErro("Bio não pode ser nulo");
        }
    }

}