using ScreenSound.Shared.Modelos.Modelos;

namespace ScreenSound.Modelos;

public class Musica : IAvaliavel<Musica>
{
    public Musica(string nome)
    {
        Nome = nome;
    }

    public string Nome { get; set; }
    public virtual ICollection<Avaliacao> Notas { get; set; } = new List<Avaliacao>();
    public int Id { get; set; }
    public int? AnoLancamento { get; set; }
    public virtual Artista? Artista { get; set; }
    public virtual ICollection<Genero> Generos { get; set; } = new List<Genero>();
    public int? ArtistaId { get; set; }
    public virtual Album? Album { get; set; }
    public int? Duracao { get; set; }

    public override string ToString()
    {
        return @$"Id: {Id}
         Nome: {Nome}";
    }


    public Musica(Artista artista, string nome)
    {
        Artista = artista;
        Nome = nome;
    }
    public Musica(int? artistaId, string nome)
    {
        ArtistaId = artistaId;
        Nome = nome;
    }

    public Musica(string nome, int anolancamento, int? artistaId, List<Genero> generos )
    {
        Nome = nome;
        AnoLancamento = anolancamento;
        ArtistaId = artistaId;
        Generos = generos;
    }
    public bool? Disponivel { get; set; }

    public double Media
    {
        get
        {
            if (Notas.Count == 0) return 0;
            else return Notas.Average(a => a.Nota);
        }
    }
    public void ExibirFichaTecnica()
    {
        Console.WriteLine($"Nome: {Nome}");
        Console.WriteLine($"Artista: {Artista!.Nome}");
        Console.WriteLine($"Duração: {Duracao}");
        if (Disponivel == true)
        {
            Console.WriteLine("Disponível no plano.");
        }
        else
        {
            Console.WriteLine("Adquira o plano Plus+");
        }
    }

    public void AdicionarNota(int pessoaId, int nota)
    {
        nota = Math.Min(Math.Max(nota, 1), 5);

        Notas.Add(new Avaliacao(nota) { PessoaId = pessoaId});
    }
}