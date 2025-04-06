using ScreenSound.Banco;
using ScreenSound.Models;

namespace ScreenSound.Menus;

class MenuExibirDetalhes : Menu
{
    public override void Executar(ArtistaDAL artistaDAL)
    {

        base.Executar(artistaDAL);
        ExibirTituloDaOpcao("Exibir detalhes da banda");
        Console.Write("Digite o nome da banda que deseja conhecer melhor: ");
        string nomeDoArtista = Console.ReadLine()!;
        var artistaRecuperado = artistaDAL.RecuperarPeloNome(nomeDoArtista);
        if (artistaRecuperado is not null)
        {
            Console.WriteLine($"\nDiscografia: ");
            artistaRecuperado.ExibirDiscografia();
            Console.WriteLine("\nDigite uma tecla para votar ao menu principal");
            Console.ReadKey();
            Console.Clear();
        }
        else
        {
            Console.WriteLine($"\nA banda {nomeDoArtista} não foi encontrada!");
            Console.WriteLine("Digite uma tecla para voltar ao menu principal");
            Console.ReadKey();
            Console.Clear();


        }
    }
}
