namespace ScreenSound.API.Dados;

/// <summary>Dados usados para popular um ambiente novo (ex.: docker compose).</summary>
public static class CatalogoDemo
{
    public static readonly (string Nome, string Bio)[] Artistas =
    {
        ("Queen", "Banda britânica formada em Londres em 1970, dona de hinos que atravessam gerações."),
        ("The Weeknd", "Cantor e produtor canadense que mistura R&B, pop e synthwave."),
        ("Daft Punk", "Duo francês de música eletrônica, pioneiro do french house."),
        ("Michael Jackson", "O Rei do Pop."),
        ("Nirvana", "Banda de Seattle que levou o grunge ao mainstream nos anos 90."),
        ("Eagles", "Banda californiana de country rock formada em 1971."),
        ("Dua Lipa", "Cantora pop britânica conhecida pela estética disco moderna."),
        ("Coldplay", "Banda britânica de rock alternativo formada em Londres em 1996."),
        ("The Killers", "Banda de rock de Las Vegas formada em 2001."),
        ("Imagine Dragons", "Banda de pop rock de Las Vegas."),
        ("Muse", "Trio britânico de rock alternativo conhecido por shows grandiosos."),
        ("Avicii", "DJ e produtor sueco que popularizou a fusão entre EDM e folk.")
    };

    public static readonly (string Nome, string Artista, int Ano, string[] Generos, string? SpotifyId)[] Faixas =
    {
        ("Bohemian Rhapsody", "Queen", 1975, new[] { "Rock", "Clássicos" }, "7tFiyTwD0nx5a1eklYtX2J"),
        ("Blinding Lights", "The Weeknd", 2019, new[] { "Pop", "Synthwave" }, "0VjIjW4GlUZAMYd2vXMi3b"),
        ("Get Lucky", "Daft Punk", 2013, new[] { "Eletrônica", "Disco" }, "69kOkLUCkxIZYexIgSG8rq"),
        ("Billie Jean", "Michael Jackson", 1982, new[] { "Pop", "Clássicos" }, "5ChkMS8OtdzJeqyybCc9R5"),
        ("Smells Like Teen Spirit", "Nirvana", 1991, new[] { "Rock", "Grunge" }, "5ghIJDpPoe3CfHMGu71E6T"),
        ("Hotel California", "Eagles", 1976, new[] { "Rock", "Clássicos" }, "40riOy7x9W7GXjyGp4pjAv"),
        ("Levitating", "Dua Lipa", 2020, new[] { "Pop", "Disco" }, "463CkQjx2Zk1yXoBuierM9"),
        ("Yellow", "Coldplay", 2000, new[] { "Rock Alternativo" }, "3AJwUDP919kvQ9QcozQPxg"),
        ("Mr. Brightside", "The Killers", 2004, new[] { "Rock Alternativo", "Indie" }, "3n3Ppam7vgaVa1iaRUc9Lp"),
        ("Believer", "Imagine Dragons", 2017, new[] { "Pop", "Rock Alternativo" }, "0pqnGHJpmpxLKifKRmU6WP"),
        ("Thunder", "Imagine Dragons", 2017, new[] { "Pop" }, "1zB4vmk8tFRmM9UULNzbLB"),
        ("Time is Running Out", "Muse", 2003, new[] { "Rock Alternativo" }, "2takcwOaAZWiXQijPHIx7B"),
        ("Wake Me Up", "Avicii", 2013, new[] { "Eletrônica" }, "0nrRP2bk19rLc0orkWPQk2"),
        ("Starboy", "The Weeknd", 2016, new[] { "Pop", "R&B" }, null)
    };

    public static readonly (string Nome, string Descricao, string[] Faixas)[] Playlists =
    {
        ("Para codar à noite", "Batidas para manter o foco depois do expediente.",
            new[] { "Get Lucky", "Blinding Lights", "Wake Me Up", "Levitating", "Time is Running Out" }),
        ("Clássicos do rock", "Os hinos que não podem faltar.",
            new[] { "Bohemian Rhapsody", "Hotel California", "Smells Like Teen Spirit", "Mr. Brightside" })
    };
}
