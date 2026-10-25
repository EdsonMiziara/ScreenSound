using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ScreenSound.API.Requests;
using ScreenSound.API.Response;
using ScreenSound.API.Seguranca;
using ScreenSound.Shared.Data.Modelos;
using System.Security.Claims;

namespace ScreenSound.API.Endpoints;

public static class UsuarioExtensions
{
    public static void AddEndpointsUsuarios(this WebApplication app)
    {
        var grupo = app.MapGroup("/Usuarios")
            .RequireCors("wasm")
            .RequireAuthorization(Politicas.Admin)
            .WithTags("Usuarios");

        grupo.MapGet("/", async ([FromServices] UserManager<PessoaComAcesso> usuarios, [FromServices] IOptions<OpcoesAcesso> opcoes) =>
        {
            var admins = (await usuarios.GetUsersInRoleAsync(Perfis.Administrador)).Select(p => p.Id).ToHashSet();
            var pessoas = await usuarios.Users.AsNoTracking().OrderBy(p => p.Email).ToListAsync();

            var resposta = pessoas.Select(p => new UsuarioResponse(
                p.Id,
                p.Email ?? string.Empty,
                p.NomePublico,
                p.FotoPerfil,
                admins.Contains(p.Id),
                opcoes.Value.EhContaDemo(p.Email)));

            return Results.Ok(resposta);
        });

        grupo.MapPut("/{id:int}/Admin", async (
            int id,
            ClaimsPrincipal user,
            [FromServices] UserManager<PessoaComAcesso> usuarios,
            [FromServices] IOptions<OpcoesAcesso> opcoes,
            [FromBody] AlterarAdminRequest request) =>
        {
            var pessoa = await usuarios.FindByIdAsync(id.ToString());
            if (pessoa is null) return Results.NotFound();

            if (usuarios.GetUserId(user) == id.ToString() && !request.Admin)
            {
                return Results.BadRequest("Você não pode remover o seu próprio acesso de administrador.");
            }

            if (opcoes.Value.EhContaDemo(pessoa.Email))
            {
                return Results.BadRequest("O perfil das contas de demonstração é fixo.");
            }

            var ehAdmin = await usuarios.IsInRoleAsync(pessoa, Perfis.Administrador);
            var resultado = request.Admin switch
            {
                true when !ehAdmin => await usuarios.AddToRoleAsync(pessoa, Perfis.Administrador),
                false when ehAdmin => await usuarios.RemoveFromRoleAsync(pessoa, Perfis.Administrador),
                _ => IdentityResult.Success
            };

            if (!resultado.Succeeded) return Results.BadRequest(string.Join(" ", resultado.Errors.Select(e => e.Description)));

            // Invalida o cookie da pessoa para que o novo perfil valha no próximo acesso
            await usuarios.UpdateSecurityStampAsync(pessoa);
            return Results.NoContent();
        });
    }
}
