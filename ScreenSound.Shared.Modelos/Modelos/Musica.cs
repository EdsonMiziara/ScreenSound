using ScreenSound.Shared.Modelos.Avaliacao;
using ScreenSound.Shared.Modelos.Validacao;

namespace ScreenSound.Modelos;

public class Musica : Valida, IAvaliavel<Musica>
{
    public Musica(string nome)
    {
        Nome = nome;
        Validar();
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
        Validar();
    }
    public Musica(int? artistaId, string nome)
    {
        ArtistaId = artistaId;
        Nome = nome;
        Validar();
    }

    public Musica(string nome, int anolancamento, int? artistaId, List<Genero> generos )
    {
        Nome = nome;
        AnoLancamento = anolancamento;
        ArtistaId = artistaId;
        Generos = generos;
        Validar();
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

    protected override void Validar()
    {
        if (Nome is null || Nome == "")
        {
            Erros.RegistrarErro("Nome não pode ser nulo ou vazio");
        }
        else if (Artista is not null && !Artista.EhValido)
        {
            Erros.RegistrarErro("Artista adicionado não pode ser invalido");
        }
    }
}