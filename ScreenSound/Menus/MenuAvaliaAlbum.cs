//namespace ScreenSound.Menus;

//using ScreenSound.Banco;
//using ScreenSound.Modelos;
//using ScreenSound.Shared.Modelos.Modelos.Avaliacao;

//class MenuAvaliaAlbum : Menu
//{
//    public override void Executar(DAL<Artista> artistaDAL)
//    {
//        base.Executar(artistaDAL);
//        ExibirTituloDaOpcao("Avaliar album");
//        Console.Write("Digite o nome da banda que deseja avaliar: ");
//        string nomeDaBanda = Console.ReadLine()!;
//        var artistaRecuperado = artistaDAL.RecuperarPor(a => a.Nome == nomeDaBanda);
//        if (artistaRecuperado is not null)
//        {
//            Artista artista = artistaRecuperado;
//            Console.Write("Digite o nome do album que deseja avaliar: ");
//            string nomeDoAlbum = Console.ReadLine()!;
//            if (artista.Albuns.Any(a => a.Nome.Equals(nomeDoAlbum)))
//            {
//                Album album = artista.Albuns.First(a => a.Nome.Equals(nomeDoAlbum));
//                Console.Write($"\nQual a nota que o Album {nomeDoAlbum} merece: ");
//                Avaliacao nota = Avaliacao.Parse(Console.ReadLine()!);
//                album.AdicionarNota(nota);
//                Console.WriteLine($"\nA nota {nota.Nota} foi registrada com sucesso para o album {nomeDoAlbum}");
//                artistaDAL.Atualizar(artistaRecuperado);
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

