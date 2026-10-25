using System.Globalization;
using System.Text;

namespace ScreenSound.Web.Services;

/// <summary>
/// Utilitários visuais determinísticos: a mesma entrada sempre gera a mesma cor, waveform e iniciais.
/// </summary>
public static class Visual
{
    private const string AvatarPadraoApi = "https://cdn.pixabay.com/photo/2016/08/08/09/17/avatar-1577909_1280.png";

    private static readonly (string De, string Para)[] Gradientes =
    {
        ("#ff5500", "#ffb300"),
        ("#7f00ff", "#e100ff"),
        ("#00c6ff", "#0072ff"),
        ("#f953c6", "#b91d73"),
        ("#11998e", "#38ef7d"),
        ("#fc4a1a", "#f7b733"),
        ("#8e2de2", "#4a00e0"),
        ("#ee0979", "#ff6a00"),
        ("#1fa2ff", "#12d8fa"),
        ("#c94b4b", "#4b134f"),
        ("#00b09b", "#96c93d"),
        ("#ff512f", "#dd2476"),
    };

    private static readonly Dictionary<(int, int), string> cacheWaveform = new();

    /// <summary>Hash FNV-1a estável entre sessões (string.GetHashCode é aleatório por processo).</summary>
    public static uint Hash(string? texto)
    {
        unchecked
        {
            var hash = 2166136261;
            foreach (var c in texto ?? string.Empty)
            {
                hash = (hash ^ c) * 16777619;
            }
            return hash;
        }
    }

    public static string Gradiente(string? semente, int angulo = 135)
    {
        var (de, para) = Gradientes[Hash(semente) % Gradientes.Length];
        return $"linear-gradient({angulo}deg, {de} 0%, {para} 100%)";
    }

    public static string CorPrincipal(string? semente) => Gradientes[Hash(semente) % Gradientes.Length].De;

    public static string Iniciais(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome)) return "?";

        var partes = nome.Split(new[] { ' ', '.', '_', '-', '@' }, StringSplitOptions.RemoveEmptyEntries);
        var iniciais = partes.Length == 1
            ? partes[0][..1]
            : $"{partes[0][0]}{partes[1][0]}";
        return iniciais.ToUpperInvariant();
    }

    /// <summary>
    /// Retorna a URL da foto do artista, ou null quando ele usa o avatar genérico (aí exibimos as iniciais).
    /// </summary>
    public static string? FotoArtista(string? fotoPerfil, string? apiUrl)
    {
        if (string.IsNullOrWhiteSpace(fotoPerfil) || fotoPerfil == AvatarPadraoApi) return null;
        if (fotoPerfil.StartsWith('/')) return $"{apiUrl?.TrimEnd('/')}{fotoPerfil}";
        return fotoPerfil;
    }

    public static string FormatarTempo(double milissegundos)
    {
        if (milissegundos <= 0 || double.IsNaN(milissegundos)) return "0:00";
        var tempo = TimeSpan.FromMilliseconds(milissegundos);
        return tempo.TotalHours >= 1 ? tempo.ToString(@"h\:mm\:ss") : tempo.ToString(@"m\:ss");
    }

    /// <summary>Fração (0..1) como porcentagem CSS, sempre com ponto decimal independentemente da cultura.</summary>
    public static string Porcentagem(double fracao) =>
        (Math.Clamp(fracao, 0, 1) * 100).ToString("0.##", CultureInfo.InvariantCulture) + "%";

    public static string Plural(int quantidade, string singular, string plural) =>
        $"{quantidade} {(quantidade == 1 ? singular : plural)}";

    public static bool Contem(string? texto, string termo) =>
        !string.IsNullOrEmpty(texto) &&
        CultureInfo.InvariantCulture.CompareInfo.IndexOf(texto, termo,
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;

    /// <summary>
    /// Gera o SVG de uma waveform no estilo SoundCloud (barras + reflexo), sempre igual para a mesma semente.
    /// </summary>
    public static string Waveform(int semente, int barras)
    {
        lock (cacheWaveform)
        {
            if (cacheWaveform.TryGetValue((semente, barras), out var svg)) return svg;

            var aleatorio = new Random(semente * 7919 + 17);
            var brutos = Enumerable.Range(0, barras).Select(_ => 0.18 + aleatorio.NextDouble() * 0.82).ToArray();
            var fase = aleatorio.NextDouble() * Math.PI * 2;

            var sb = new StringBuilder();
            sb.Append($"<svg viewBox=\"0 0 {barras * 3} 100\" preserveAspectRatio=\"none\" aria-hidden=\"true\">");

            for (var i = 0; i < barras; i++)
            {
                // suaviza com os vizinhos e aplica um "envelope" para parecer uma música de verdade
                var vizinhos = brutos[Math.Max(0, i - 1)] + brutos[i] * 2 + brutos[Math.Min(barras - 1, i + 1)];
                var posicao = (double)i / barras;
                var envelope = Math.Min(1, Math.Min(posicao / 0.06, (1 - posicao) / 0.06));
                var dinamica = 0.65 + 0.35 * Math.Sin(posicao * Math.PI * 3 + fase);
                var altura = Math.Clamp(vizinhos / 4 * Math.Max(0.15, envelope) * dinamica, 0.06, 1);

                var superior = altura * 68;
                var inferior = altura * 28;
                var x = (i * 3).ToString(CultureInfo.InvariantCulture);
                sb.Append(CultureInfo.InvariantCulture,
                    $"<rect class=\"u\" x=\"{x}\" y=\"{70 - superior:0.##}\" width=\"2\" height=\"{superior:0.##}\"/>");
                sb.Append(CultureInfo.InvariantCulture,
                    $"<rect class=\"l\" x=\"{x}\" y=\"71\" width=\"2\" height=\"{inferior:0.##}\"/>");
            }

            sb.Append("</svg>");
            svg = sb.ToString();
            cacheWaveform[(semente, barras)] = svg;
            return svg;
        }
    }
}
