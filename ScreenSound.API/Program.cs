using Microsoft.AspNetCore.Mvc;
using ScreenSound.API.Endpoints;
using System.Text.Json.Serialization;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ScreenSound.Shared.Data.Modelos;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);


var allowedOrigins = builder.Configuration.GetValue<string>("Cors:AllowedOrigins");

builder.Services.AddCors(options =>
{
    options.AddPolicy("wasm", policy =>
    {
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
    options.UseSqlServer(builder.Configuration["ConnectionStrings:ScreenSoundDB"])
                .UseLazyLoadingProxies();
});

builder.Services
    .AddIdentityApiEndpoints<PessoaComAcesso>()
    .AddEntityFrameworkStores<ScreenSoundContext>();

builder.Services.AddAuthorization();

builder.Services.AddScoped<DAL<Artista>>();
builder.Services.AddScoped<DAL<Musica>>();
builder.Services.AddScoped<DAL<Genero>>();
builder.Services.AddScoped<DAL<Album>>();
builder.Services.AddScoped<DAL<PessoaComAcesso>>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
var app = builder.Build();



app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FotosPerfil")),
    RequestPath = "/FotosPerfil"
});
app.UseRouting(); // Certifique-se de que UseRouting está aqui

// Use o middleware do CORS
app.UseCors("wasm");


app.UseAuthentication();
app.UseAuthorization();


app.AddEndPointsArtistas();
app.AddEndPointsMusicas();
app.AddEndpointsGenero();
app.AddEndpointsAlbuns();

app.MapGroup("auth").MapIdentityApi<PessoaComAcesso>().WithTags("Autorizacao");

app.MapPost("auth/logout", async ([FromServices] SignInManager<PessoaComAcesso> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok();
}).RequireAuthorization().WithTags("Autorizacao");


app.UseSwagger();
app.UseSwaggerUI();

app.UseEndpoints(endpoints =>
{
    // Você pode adicionar endpoints adicionais aqui, se necessário.
    // Para Minimal APIs definidas com MapGroup, isso geralmente não é necessário.
});

app.Run();

public partial class Program { }