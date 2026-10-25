namespace ScreenSound.Teste.SpotifyLink;

using ScreenSound.Shared.Modelos.Auxiliar;

public class SpotifyLinkTest
{

    [Theory]
    [InlineData("https://open.spotify.com/track/4uLU6hMCjMI75M1A2tKUQC?si=abc", true)]
    [InlineData("https://open.spotify.com/intl-pt/track/4uLU6hMCjMI75M1A2tKUQC", true)]
    [InlineData("https://exemplo.com/track/123", false)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData(null, true)]
    public void SpotifyLink_ValidaCorretamente(string? link, bool esperado)
    {
        Assert.Equal(esperado, SpotifyLink.EhValido(link));
    }

    [Theory]
    [InlineData("https://open.spotify.com/intl-pt/track/4uLU6hMCjMI75M1A2tKUQC?si=abc", "spotify:track:4uLU6hMCjMI75M1A2tKUQC")]
    [InlineData("https://open.spotify.com/album/1ATL5GLyefJaxhQzSPVrLX", "spotify:album:1ATL5GLyefJaxhQzSPVrLX")]
    [InlineData("https://exemplo.com/track/123", null)]
    [InlineData(null, null)]
    public void SpotifyLink_ConverteParaUri(string? link, string? esperado)
    {
        Assert.Equal(esperado, SpotifyLink.ParaUri(link));
    }

    [Fact]
    public void SpotifyLink_ConverteParaCanonico()
    {
        var canonico = SpotifyLink.ParaCanonico("https://open.spotify.com/intl-pt/track/4uLU6hMCjMI75M1A2tKUQC?si=abc");
        Assert.Equal("https://open.spotify.com/track/4uLU6hMCjMI75M1A2tKUQC", canonico);
    }
}
