using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ScreenSound.Shared.Modelos.Modelos;


public class Avaliacao
{
    public Avaliacao(int nota)
    {        
            Nota = Math.Clamp(nota, 1, 5);       
    }
    public int Nota { get; set; }
    public int? PessoaId { get; set; }
    public int Id { get; set; }

    public static Avaliacao Parse(string texto)
    {
        int nota = int.Parse(texto);
        return new Avaliacao(nota);
    }
}