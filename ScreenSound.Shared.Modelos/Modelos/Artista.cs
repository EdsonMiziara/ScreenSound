﻿namespace ScreenSound.Modelos;

public class Artista : IAvaliavel
{
    public virtual ICollection<Album> Albuns { get; set; } = new List<Album>();
    public virtual ICollection<Musica> Musicas { get; set; } = new List<Musica>();
    public virtual ICollection<Avaliacao> Notas { get; set; } = new List<Avaliacao>();


    public Artista(string nome, string bio)
    {
        Nome = nome;
        Bio = bio;
        FotoPerfil = "https://cdn.pixabay.com/photo/2016/08/08/09/17/avatar-1577909_1280.png";
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
        Albuns.Add(album);
    }

    public void AdicionarNota(Avaliacao nota)
    {
        Notas.Add(nota);
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

}