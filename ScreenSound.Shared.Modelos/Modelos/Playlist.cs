using ScreenSound.Shared.Modelos.Validacao;

namespace ScreenSound.Modelos;

public class Playlist : Valida
{
    public Playlist(string nome, int pessoaId)
    {
        Nome = nome;
        PessoaId = pessoaId;
        Validar();
    }

    public int Id { get; set; }
    public string Nome { get; set; }
    public string? Descricao { get; set; }
    public bool Publica { get; set; } = true;
    public int PessoaId { get; set; }
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;
    public virtual ICollection<PlaylistMusica> Itens { get; set; } = new List<PlaylistMusica>();

    public bool Contem(int musicaId) => Itens.Any(i => i.MusicaId == musicaId);

    /// <summary>
    /// Adiciona a música no fim da playlist. Retorna false se ela já estiver na playlist.
    /// </summary>
    public bool AdicionarMusica(int musicaId)
    {
        if (Contem(musicaId)) return false;

        var proximaOrdem = Itens.Count == 0 ? 0 : Itens.Max(i => i.Ordem) + 1;
        Itens.Add(new PlaylistMusica { MusicaId = musicaId, Ordem = proximaOrdem });
        return true;
    }

    public bool RemoverMusica(int musicaId)
    {
        var item = Itens.FirstOrDefault(i => i.MusicaId == musicaId);
        if (item is null) return false;

        Itens.Remove(item);
        return true;
    }

    /// <summary>
    /// Reordena a playlist conforme a sequência de ids informada.
    /// Ids desconhecidos são ignorados e músicas não informadas vão para o fim, na ordem atual.
    /// </summary>
    public void Reordenar(IEnumerable<int> musicaIds)
    {
        var posicoes = musicaIds
            .Distinct()
            .Select((id, indice) => (id, indice))
            .ToDictionary(p => p.id, p => p.indice);

        var ordenados = Itens
            .OrderBy(i => posicoes.TryGetValue(i.MusicaId, out var p) ? p : int.MaxValue)
            .ThenBy(i => i.Ordem)
            .ToList();

        for (var i = 0; i < ordenados.Count; i++)
        {
            ordenados[i].Ordem = i;
        }
    }

    protected override void Validar()
    {
        if (string.IsNullOrWhiteSpace(Nome))
        {
            Erros.RegistrarErro("O nome da playlist não pode ser nulo ou vazio.");
        }
        else if (Nome.Length > 100)
        {
            Erros.RegistrarErro("O nome da playlist deve ter no máximo 100 caracteres.");
        }
    }
}

public class PlaylistMusica
{
    public int PlaylistId { get; set; }
    public virtual Playlist? Playlist { get; set; }
    public int MusicaId { get; set; }
    public virtual Musica? Musica { get; set; }
    public int Ordem { get; set; }
    public DateTime AdicionadaEm { get; set; } = DateTime.UtcNow;
}
