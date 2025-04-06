namespace ScreenSound.Models;

internal class Musica /*: IAvaliavel */
{
    public Musica(string nome)
    {
        Nome = nome;
    }

    public string Nome { get; set; }
    public int Id { get; set; }

    public void ExibirFichaTecnica()
    {
        Console.WriteLine($"Nome: {Nome}");

    }

    public override string ToString()
    {
        return @$"Id: {Id}
        Nome: {Nome}";
    }


    //public Musica(Artista artista, string nome)
    //{
    //    Artista = artista;
    //    Nome = nome;
    //}

    //public Artista Artista { get; }
    //public int Duracao { get; set; }
    //public bool Disponivel { get; set; }
    //public string DescricaoResumida => $"A música {Nome} pertence à banda {Artista}";

    //public double Media => throw new NotImplementedException();

    //public void AdicionarNota(Avaliacao nota)
    //{
    //    throw new NotImplementedException();
    //}

    //public void ExibirFichaTecnica()
    //{
    //    Console.WriteLine($"Nome: {Nome}");
    //    Console.WriteLine($"Artista: {Artista.Nome}");
    //    Console.WriteLine($"Duração: {Duracao}");
    //    if (Disponivel)
    //    {
    //        Console.WriteLine("Disponível no plano.");
    //    } else
    //    {
    //        Console.WriteLine("Adquira o plano Plus+");
    //    }
    //}
}