namespace ScreenSound.Models;

public class Musica : IAvaliavel
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
    public virtual Album? Album { get; set; }
    public int Duracao { get; set; }

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

    public bool Disponivel { get; set; }

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
        Console.WriteLine($"Artista: {Artista.Nome}");
        Console.WriteLine($"Duração: {Duracao}");
        if (Disponivel)
        {
            Console.WriteLine("Disponível no plano.");
        }
        else
        {
            Console.WriteLine("Adquira o plano Plus+");
        }
    }

    public void AdicionarNota(Avaliacao nota)
    {
        Notas.Add(nota);
    }
}