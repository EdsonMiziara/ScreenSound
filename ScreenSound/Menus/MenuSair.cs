using ScreenSound.Banco;
using ScreenSound.Models;

namespace ScreenSound.Menus;

class MenuSair : Menu
{
    public override void Executar(ArtistaDAL artistaDAL)
    {
        Console.WriteLine("Tchau tchau :)");
    }
}
