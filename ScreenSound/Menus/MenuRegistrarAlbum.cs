using ScreenSound.Banco;
using ScreenSound.Modelos;

namespace ScreenSound.Menus;

class MenuRegistrarAlbum : Menu
{
    public override void Executar(DAL<Artista> artistaDAL)
    {
        base.Executar(artistaDAL);
        ExibirTituloDaOpcao("Registro de álbuns");
        Console.Write("Digite o Artista cujo álbum deseja registrar: ");
        string nomeDoArtista = Console.ReadLine()!;
        var artistaRecuperado = artistaDAL.RecuperarPor(a => a.Nome.Equals(nomeDoArtista));
        if (artistaRecuperado is not null)
        {
            Console.Write("Agora digite o título do álbum: ");
            string tituloAlbum = Console.ReadLine()!;
            artistaRecuperado.AdicionarAlbum(new Album(tituloAlbum));

            Console.WriteLine($"O álbum {tituloAlbum} de {nomeDoArtista} foi registrado com sucesso!");
            artistaDAL.Atualizar(artistaRecuperado);
            Thread.Sleep(4000);
            Console.Clear();
        }
        else
        {
            Console.WriteLine($"\no artista {nomeDoArtista} não foi encontrada!");
            Console.WriteLine("Digite uma tecla para voltar ao menu principal");
            Console.ReadKey();
            Console.Clear();
        }

    }
}
