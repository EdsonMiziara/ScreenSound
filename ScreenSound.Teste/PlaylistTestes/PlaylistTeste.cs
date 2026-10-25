using ScreenSound.Modelos;

namespace ScreenSound.Teste.PlaylistTestes;

public class PlaylistTeste
{
    [Theory]
    [InlineData("Para treinar", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void Playlist_ValidaNome(string nome, bool esperado)
    {
        var playlist = new Playlist(nome, pessoaId: 1);

        Assert.Equal(esperado, playlist.EhValido);
    }

    [Fact]
    public void Playlist_NaoAceitaNomeMaiorQue100Caracteres()
    {
        var playlist = new Playlist(new string('a', 101), pessoaId: 1);

        Assert.False(playlist.EhValido);
    }

    [Fact]
    public void AdicionarMusica_ColocaNoFimENaoDuplica()
    {
        var playlist = new Playlist("Favoritas", 1);

        Assert.True(playlist.AdicionarMusica(10));
        Assert.True(playlist.AdicionarMusica(20));
        Assert.False(playlist.AdicionarMusica(10));

        Assert.Equal(2, playlist.Itens.Count);
        Assert.Equal(new[] { 10, 20 }, playlist.Itens.OrderBy(i => i.Ordem).Select(i => i.MusicaId));
    }

    [Fact]
    public void RemoverMusica_RetornaFalseQuandoNaoExiste()
    {
        var playlist = new Playlist("Favoritas", 1);
        playlist.AdicionarMusica(10);

        Assert.False(playlist.RemoverMusica(99));
        Assert.True(playlist.RemoverMusica(10));
        Assert.Empty(playlist.Itens);
    }

    [Fact]
    public void Reordenar_AplicaNovaOrdemEMantemNaoInformadasNoFim()
    {
        var playlist = new Playlist("Favoritas", 1);
        playlist.AdicionarMusica(1);
        playlist.AdicionarMusica(2);
        playlist.AdicionarMusica(3);
        playlist.AdicionarMusica(4);

        playlist.Reordenar(new[] { 3, 1, 999 });

        var ordem = playlist.Itens.OrderBy(i => i.Ordem).Select(i => i.MusicaId);
        Assert.Equal(new[] { 3, 1, 2, 4 }, ordem);
    }
}
