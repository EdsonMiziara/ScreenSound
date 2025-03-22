using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ScreenSound.Models;


internal class Avaliacao
{
    public Avaliacao(int nota)
    {
        if(nota <= 10 && nota >= 0)
        {
            Nota = nota;
        }
        else
        {
            Console.WriteLine("Nota invalida");
        }
    }
    public int Nota { get; }

    public static Avaliacao Parse(string texto)
    {
        int nota = int.Parse(texto);
        return new Avaliacao(nota);
    }
}
