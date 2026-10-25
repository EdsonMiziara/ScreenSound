using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ScreenSound.API.Requests;
using ScreenSound.API.Response;
using ScreenSound.API.Seguranca;
using ScreenSound.Shared.Data.Modelos;
using System.Security.Claims;

namespace ScreenSound.API.Endpoints;

public static class PerfilExtensions
{
    private const int TamanhoMaximoFoto = 2 * 1024 * 1024;

    public static void AddEndpointsPerfil(this WebApplication app)
    {
        var grupo = app.MapGroup("/Perfil")
            .RequireCors("wasm")
            .RequireAuthorization()
            .WithTags("Perfil");

        grupo.MapGet("/", async (ClaimsPrincipal user, [FromServices] UserManager<PessoaComAcesso> usuarios, [FromServices] IOptions<OpcoesAcesso> opcoes) =>
        {
            var pessoa = await usuarios.GetUserAsync(user);
            if (pessoa is null) return Results.Unauthorized();

            return Results.Ok(await ParaResponseAsync(pessoa, usuarios, opcoes.Value));
        });

        grupo.MapPut("/", async (
            ClaimsPrincipal user,
            [FromServices] UserManager<PessoaComAcesso> usuarios,
            [FromServices] IOptions<OpcoesAcesso> opcoes,
            [FromBody] PerfilRequest request) =>
        {
            var pessoa = await usuarios.GetUserAsync(user);
            if (pessoa is null) return Results.Unauthorized();

            var nome = request.NomeExibicao?.Trim();
            var bio = request.Bio?.Trim();
            if (nome is { Length: > 60 }) return Results.BadRequest("O nome de exibição deve ter no máximo 60 caracteres.");
            if (bio is { Length: > 300 }) return Results.BadRequest("A bio deve ter no máximo 300 caracteres.");

            pessoa.NomeExibicao = string.IsNullOrWhiteSpace(nome) ? null : nome;
            pessoa.Bio = string.IsNullOrWhiteSpace(bio) ? null : bio;
            await usuarios.UpdateAsync(pessoa);

            return Results.Ok(await ParaResponseAsync(pessoa, usuarios, opcoes.Value));
        });

        grupo.MapPut("/Foto", async (
            ClaimsPrincipal user,
            [FromServices] UserManager<PessoaComAcesso> usuarios,
            [FromServices] IOptions<OpcoesAcesso> opcoes,
            [FromServices] IHostEnvironment env,
            [FromBody] FotoPerfilRequest request) =>
        {
            var pessoa = await usuarios.GetUserAsync(user);
            if (pessoa is null) return Results.Unauthorized();

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(request.FotoBase64);
            }
            catch (FormatException)
            {
                return Results.BadRequest("Imagem inválida.");
            }

            // Aceita somente JPEG (o front converte a imagem antes de enviar)
            if (bytes.Length is 0 or > TamanhoMaximoFoto || bytes.Length < 3 || bytes[0] != 0xFF || bytes[1] != 0xD8 || bytes[2] != 0xFF)
            {
                return Results.BadRequest("Envie uma imagem JPEG de até 2 MB.");
            }

            var pasta = Path.Combine(env.ContentRootPath, "wwwroot", "FotosPerfil");
            Directory.CreateDirectory(pasta);

            var arquivo = $"usuario-{pessoa.Id}-{Guid.NewGuid():N}.jpeg";
            await File.WriteAllBytesAsync(Path.Combine(pasta, arquivo), bytes);

            ApagarFoto(pessoa.FotoPerfil, env);
            pessoa.FotoPerfil = $"/FotosPerfil/{arquivo}";
            await usuarios.UpdateAsync(pessoa);

            return Results.Ok(await ParaResponseAsync(pessoa, usuarios, opcoes.Value));
        });

        grupo.MapDelete("/Foto", async (
            ClaimsPrincipal user,
            [FromServices] UserManager<PessoaComAcesso> usuarios,
            [FromServices] IOptions<OpcoesAcesso> opcoes,
            [FromServices] IHostEnvironment env) =>
        {
            var pessoa = await usuarios.GetUserAsync(user);
            if (pessoa is null) return Results.Unauthorized();

            ApagarFoto(pessoa.FotoPerfil, env);
            pessoa.FotoPerfil = null;
            await usuarios.UpdateAsync(pessoa);

            return Results.Ok(await ParaResponseAsync(pessoa, usuarios, opcoes.Value));
        });

        grupo.MapPost("/Senha", async (
            ClaimsPrincipal user,
            [FromServices] UserManager<PessoaComAcesso> usuarios,
            [FromServices] SignInManager<PessoaComAcesso> signIn,
            [FromServices] IOptions<OpcoesAcesso> opcoes,
            [FromBody] TrocaSenhaRequest request) =>
        {
            var pessoa = await usuarios.GetUserAsync(user);
            if (pessoa is null) return Results.Unauthorized();

            // Contas demo são compartilhadas: trocar a senha bloquearia os outros visitantes
            if (opcoes.Value.EhContaDemo(pessoa.Email))
            {
                return Results.BadRequest("A senha das contas de demonstração não pode ser alterada.");
            }

            var resultado = await usuarios.ChangePasswordAsync(pessoa, request.SenhaAtual, request.NovaSenha);
            if (!resultado.Succeeded)
            {
                return Results.ValidationProblem(resultado.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
            }

            await signIn.RefreshSignInAsync(pessoa);
            return Results.NoContent();
        });
    }

    internal static async Task<PerfilResponse> ParaResponseAsync(PessoaComAcesso pessoa, UserManager<PessoaComAcesso> usuarios, OpcoesAcesso opcoes)
    {
        var papeis = await usuarios.GetRolesAsync(pessoa);
        return new PerfilResponse(
            pessoa.Id,
            pessoa.Email ?? string.Empty,
            pessoa.NomeExibicao,
            pessoa.NomePublico,
            pessoa.Bio,
            pessoa.FotoPerfil,
            papeis.ToList(),
            opcoes.EhContaDemo(pessoa.Email));
    }

    private static void ApagarFoto(string? foto, IHostEnvironment env)
    {
        if (string.IsNullOrEmpty(foto) || !foto.StartsWith("/FotosPerfil/usuario-")) return;

        var caminho = Path.Combine(env.ContentRootPath, "wwwroot", "FotosPerfil", Path.GetFileName(foto));
        if (File.Exists(caminho)) File.Delete(caminho);
    }
}
