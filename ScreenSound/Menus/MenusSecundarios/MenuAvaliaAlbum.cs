//namespace ScreenSound.Menus.MenusSecundarios;
//using ScreenSound.Models;

//class MenuAvaliaAlbum : Menu
//{
//    public override void Executar(Dictionary<string, Artista> bandasRegistradas)
//    {
//        base.Executar(bandasRegistradas);
//        ExibirTituloDaOpcao("Avaliar album");
//        Console.Write("Digite o nome da banda que deseja avaliar: ");
//        string nomeDaBanda = Console.ReadLine()!;
//        if (bandasRegistradas.ContainsKey(nomeDaBanda))
//        {
//            Artista banda = bandasRegistradas[nomeDaBanda];
//            Console.Write("Digite o nome do album que deseja avaliar: ");
//            string nomeDoAlbum = Console.ReadLine()!;
//            if (banda.Albuns.Any(a => a.Nome.Equals(nomeDoAlbum)))
//            {
//                Album album = banda.Albuns.First(a => a.Nome.Equals(nomeDoAlbum));
//                Console.Write($"\nQual a nota que o Album {nomeDoAlbum} merece: ");
//                Avaliacao nota = Avaliacao.Parse(Console.ReadLine()!);
//                album.AdicionarNota(nota);
//                Console.WriteLine($"\nA nota {nota.Nota} foi registrada com sucesso para o album {nomeDoAlbum}");
//                Thread.Sleep(2000);
//                Console.Clear();
//            }
//            else
//            {
//                Console.WriteLine($"\nO album {nomeDoAlbum} não foi encontrada!");
//                Console.WriteLine("Digite uma tecla para voltar ao menu principal");
//                Console.ReadKey();
//                Console.Clear();

//            }


//        }
//        else
//        {
//            Console.WriteLine($"\nA banda {nomeDaBanda} não foi encontrada!");
//            Console.WriteLine("Digite uma tecla para voltar ao menu principal");
//            Console.ReadKey();
//            Console.Clear();


//        }
//    }
//}

