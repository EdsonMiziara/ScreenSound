﻿using ScreenSound.Shared.Modelos.Avaliacao;
using ScreenSound.Shared.Modelos.Validacao;
using System.Security.Principal;

namespace ScreenSound.Modelos;
public class Album : Valida, IAvaliavel<Album>
{
    public virtual ICollection<Musica> Musicas { get; set; } = new List<Musica>();
    public virtual ICollection<Avaliacao> Notas { get; set; } = new List<Avaliacao>();
    public virtual Artista? Artista { get; set; }
    public int? ArtistaId { get; set; }
    public Album(string nome)
    {
        Nome = nome;
        Validar();
    }

    public string Nome { get; set; }
    public int? DuracaoTotal => Musicas.Sum(m => m.Duracao);
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

    public void AdicionarNota(int pessoaId, int nota)
    {
        nota = Math.Clamp(nota, 1, 5);

        Notas.Add(new Avaliacao(nota) { PessoaId = pessoaId });
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
    protected override void Validar()
    {
        if (Nome is null || Nome == "")
        {
            Erros.RegistrarErro("Nome do álbum não pode ser nulo ou vazio.");
        }
    }
}