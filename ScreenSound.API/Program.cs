using Microsoft.AspNetCore.Mvc;
using ScreenSound.API.Endpoints;
using System.Text.Json.Serialization;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
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
app.UseCors("AllowAllOrigins");

app.AddEndPointsArtistas();
app.AddEndPointsMusicas();
app.AddEndpointsGenero();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();