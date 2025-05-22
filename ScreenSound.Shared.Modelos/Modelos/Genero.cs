using ScreenSound.Modelos;
using ScreenSound.Shared.Modelos.Validacao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenSound.Modelos;

public class Genero : Valida
{
    public Genero(string nome, string descricao)
    {
        Nome = nome;
        Descricao = descricao;
        Validar();
    }
    public Genero() { }

    public virtual ICollection<Musica> Musicas { get; set; } = new List<Musica>();
    public int Id { get; set; }
    public string? Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Nome: {Nome} - Descrição: {Descricao}";
    }
    protected override void Validar()
    {
        if (Nome is null || Nome == "")
        {
            Erros.RegistrarErro("O nome do gênero não pode ser nulo ou vazio.");
        }
    }
}