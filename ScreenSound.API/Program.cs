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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("wasm", policy =>
    {
        var allowedOrigins = builder.Configuration.GetValue<string>("Cors:AllowedOrigins");
        if (!string.IsNullOrEmpty(allowedOrigins))
        {
            policy.WithOrigins(allowedOrigins.Split(','))
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
    .AddEntityFrameworkStores<ScreenSoundContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddAuthorization();

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

app.MapGroup("auth").MapIdentityApi<PessoaComAcesso>()
    .WithTags("Autorizacao")
    .RequireCors("wasm");

app.MapPost("auth/logout", async ([FromServices] SignInManager<PessoaComAcesso> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok();
}).RequireAuthorization().WithTags("Autorizacao");

app.Run();

public partial class Program { }