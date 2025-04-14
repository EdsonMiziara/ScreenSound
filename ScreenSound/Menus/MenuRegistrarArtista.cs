using ScreenSound.Banco;
using ScreenSound.Modelos;

namespace ScreenSound.Menus;

class MenuRegistrarArtista : Menu
{
    public override void Executar(DAL<Artista> artistaDAL)
    {
        base.Executar(artistaDAL);
        ExibirTituloDaOpcao("Registro das bandas");
        Console.Write("Digite o nome do Artista que deseja registrar: ");
        string nomeDoArtista = Console.ReadLine()!;
        Console.Write("Digite a Bio do Artista que deseja registrar: ");
        string bioDoArtista = Console.ReadLine()!;
        Artista artista = new(nomeDoArtista, bioDoArtista);
        artistaDAL.Adicionar(artista);
        Console.WriteLine($"A banda {nomeDoArtista} foi registrada com sucesso!");
        Thread.Sleep(4000);
        Console.Clear();
    }
}
