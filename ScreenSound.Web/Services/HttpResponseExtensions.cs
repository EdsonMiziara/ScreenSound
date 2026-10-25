using System.Net;
using System.Text.Json;

namespace ScreenSound.Web.Services;

public static class HttpResponseExtensions
{
    /// <summary>
    /// Lança <see cref="HttpRequestException"/> com a mensagem devolvida pela API quando a resposta não é de sucesso,
    /// para que as telas possam exibir o motivo real do erro.
    /// </summary>
    public static async Task GarantirSucessoAsync(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var corpo = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException(ExtrairMensagem(corpo, response.StatusCode), null, response.StatusCode);
    }

    private static string ExtrairMensagem(string corpo, HttpStatusCode status)
    {
        if (string.IsNullOrWhiteSpace(corpo))
        {
            return status switch
            {
                HttpStatusCode.Unauthorized => "Sua sessão expirou. Entre novamente.",
                HttpStatusCode.NotFound => "Registro não encontrado.",
                _ => "Não foi possível concluir a operação."
            };
        }

        try
        {
            using var json = JsonDocument.Parse(corpo);
            var raiz = json.RootElement;

            if (raiz.ValueKind == JsonValueKind.String) return raiz.GetString()!.Trim();

            // ValidationProblemDetails (ex.: erros do Identity no cadastro)
            if (raiz.TryGetProperty("errors", out var erros) && erros.ValueKind == JsonValueKind.Object)
            {
                var mensagens = erros.EnumerateObject()
                    .SelectMany(e => e.Value.EnumerateArray().Select(v => v.GetString()))
                    .Where(m => !string.IsNullOrWhiteSpace(m));
                return string.Join(" ", mensagens);
            }

            if (raiz.TryGetProperty("title", out var titulo)) return titulo.GetString() ?? corpo;
        }
        catch (JsonException)
        {
        }

        return corpo.Trim();
    }
}
