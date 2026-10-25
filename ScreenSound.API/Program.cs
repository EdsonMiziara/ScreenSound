using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScreenSound.API.Endpoints;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using ScreenSound.Shared.Data.Banco;
using ScreenSound.Shared.Data.Modelos;
using AutoMapper;
using ScreenSound.API.Profiles;
using ScreenSound.API.Dados;
using ScreenSound.API.Seguranca;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("wasm", policy =>
    {
        var origens = (builder.Configuration["Cors:AllowedOrigins"] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(o => o.TrimEnd('/'))
            .ToArray();

        if (origens.Length > 0)
        {
            policy.WithOrigins(origens)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

builder.Services.AddDbContext<ScreenSoundContext>((options) =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:ScreenSoundDB"],
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure());

});

builder.Services.AddIdentityApiEndpoints<PessoaComAcesso>()
    .AddRoles<PerfilDeAcesso>()
    .AddEntityFrameworkStores<ScreenSoundContext>();

builder.Services.Configure<OpcoesAcesso>(builder.Configuration.GetSection("Acesso"));

builder.Services.ConfigureApplicationCookie(options =>
{
    // Padrão (Azure): front e API em domínios diferentes, cookie precisa ser SameSite=None + Secure.
    // No Docker o front faz proxy da API na mesma origem via HTTP, então "Cookies:EntreDominios" = false.
    if (builder.Configuration.GetValue("Cookies:EntreDominios", true))
    {
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    }
    else
    {
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    }
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Politicas.Admin, policy => policy.RequireRole(Perfis.Administrador));
});

builder.Services.AddScoped(typeof(DAL<>));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<ArtistaProfile>();

});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("wasm");

app.MapGet("/FotosPerfil/{fileName}", (string fileName, IHostEnvironment env) =>
{
    var filePath = Path.Combine(env.ContentRootPath, "wwwroot", "FotosPerfil", fileName);
    if (!File.Exists(filePath))
    {
        return Results.NotFound();
    }

    return Results.File(filePath, "image/jpeg");

}).AllowAnonymous();

app.UseAuthentication();
app.UseAuthorization();

app.AddEndPointsArtistas();
app.AddEndPointsMusicas();
app.AddEndpointsGenero();
app.AddEndpointsAlbuns();
app.AddEndpointsPlaylists();
app.AddEndpointsPerfil();
app.AddEndpointsUsuarios();

app.MapGroup("auth").MapIdentityApi<PessoaComAcesso>()
    .WithTags("Autorizacao")
    .RequireCors("wasm");

app.MapPost("auth/logout", async ([FromServices] SignInManager<PessoaComAcesso> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok();
}).RequireAuthorization().WithTags("Autorizacao");

await app.AplicarMigracoesAsync();
await app.InicializarAcessoAsync();
await app.PopularCatalogoDemoAsync();

app.Run();

public partial class Program { }