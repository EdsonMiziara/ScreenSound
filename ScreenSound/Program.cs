using ScreenSound.Banco;
using ScreenSound.Menus;
using ScreenSound.Models;

//Ira.AdicionarNota(new Avaliacao(10));
//Ira.AdicionarNota(new Avaliacao(8));
//Ira.AdicionarNota(new Avaliacao(6));
var context = new ScreenSoundContext();
var artistaDAL = new DAL<Artista>(context);

Dictionary<int, Menu> opcoes = new();
opcoes.Add(1, new MenuRegistrarArtista());
opcoes.Add(2, new MenuRegistrarMusica());
opcoes.Add(3, new MenuExibirDetalhes());
opcoes.Add(4, new MenuMostrarArtistas());
opcoes.Add(-1, new MenuSair());
//opcoes.Add(2, new MenuRegistrarAlbum());
//opcoes.Add(4, new MenuAvaliarBanda());
//opcoes.Add(5, new MenuAvaliaAlbum());
//opcoes.Add(6, new MenuExibirDetalhes());

void ExibirLogo()
{
    Console.WriteLine(@"

░██████╗░█████╗░██████╗░███████╗███████╗███╗░░██╗  ░██████╗░█████╗░██╗░░░██╗███╗░░██╗██████╗░
██╔════╝██╔══██╗██╔══██╗██╔════╝██╔════╝████╗░██║  ██╔════╝██╔══██╗██║░░░██║████╗░██║██╔══██╗
╚█████╗░██║░░╚═╝██████╔╝█████╗░░█████╗░░██╔██╗██║  ╚█████╗░██║░░██║██║░░░██║██╔██╗██║██║░░██║
░╚═══██╗██║░░██╗██╔══██╗██╔══╝░░██╔══╝░░██║╚████║  ░╚═══██╗██║░░██║██║░░░██║██║╚████║██║░░██║
██████╔╝╚█████╔╝██║░░██║███████╗███████╗██║░╚███║  ██████╔╝╚█████╔╝╚██████╔╝██║░╚███║██████╔╝
╚═════╝░░╚════╝░╚═╝░░╚═╝╚══════╝╚══════╝╚═╝░░╚══╝  ╚═════╝░░╚════╝░░╚═════╝░╚═╝░░╚══╝╚═════╝░
");
    Console.WriteLine("Boas vindas ao Screen Sound 2.0!");
}

void ExibirOpcoesDoMenu()
{
    ExibirLogo();
    Console.WriteLine("\nDigite 1 para registrar um Artista");
    Console.WriteLine("Digite 2 para Registrar uma musica");
    Console.WriteLine("Digite 3 para exibir os detalhes de um Artista");
    Console.WriteLine("Digite 4 para mostrar todas os Artistas");
    Console.WriteLine("Digite -1 para sair");
    //Console.WriteLine("Digite 2 para registrar o álbum de uma banda");
    //Console.WriteLine("Digite 4 para avaliar uma banda");
    //Console.WriteLine("Digite 5 para avaliar um album");

    Console.Write("\nDigite a sua opção: ");
    string opcaoEscolhida = Console.ReadLine()!;
    int opcaoEscolhidaNumerica = int.Parse(opcaoEscolhida);

    if (opcoes.ContainsKey(opcaoEscolhidaNumerica))
    {
        Menu menu = opcoes[opcaoEscolhidaNumerica];
        menu.Executar(artistaDAL);
        if (opcaoEscolhidaNumerica > 0) ExibirOpcoesDoMenu();
    }
    else
    {
        Console.WriteLine("Opção inválida");
    }

}

ExibirOpcoesDoMenu();