using Microsoft.AspNetCore.Mvc;
using ScreenSound.API.Endpoints;
using System.Text.Json.Serialization;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("https://localhost:7265")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<ScreenSoundContext>((options) =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:ScreenSoundDB"])
           .UseLazyLoadingProxies();
});
builder.Services.AddTransient<DAL<Artista>>();
builder.Services.AddTransient<DAL<Musica>>();
builder.Services.AddTransient<DAL<Genero>>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
var app = builder.Build();

// Use o middleware do CORS
app.UseCors("AllowSpecificOrigin");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FotosPerfil")),
    RequestPath = "/FotosPerfil"
});


app.AddEndPointsArtistas();
app.AddEndPointsMusicas();
app.AddEndpointsGenero();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();