using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using ScreenSound.Web;
using ScreenSound.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
    config.SnackbarConfiguration.VisibleStateDuration = 3500;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.PreventDuplicates = true;
});

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AuthAPI>();
builder.Services.AddScoped<AuthAPI>(sp => (AuthAPI)sp.GetRequiredService<AuthenticationStateProvider>());


builder.Services.AddScoped<CookieHandler>();
builder.Services.AddScoped<MusicaAPI>();
builder.Services.AddScoped<ArtistaAPI>();
builder.Services.AddScoped<GeneroAPI>();
builder.Services.AddScoped<PlaylistAPI>();
builder.Services.AddScoped<PerfilAPI>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<CapaService>();


// APIServer:Url pode ser absoluta (Azure / desenvolvimento) ou relativa ao site (Docker: "api/", via proxy do nginx)
var urlApi = builder.Configuration["APIServer:Url"]!;
var baseApi = Uri.IsWellFormedUriString(urlApi, UriKind.Absolute)
    ? new Uri(urlApi)
    : new Uri(new Uri(builder.HostEnvironment.BaseAddress), urlApi);

builder.Services.AddHttpClient("API", client =>
{

    client.BaseAddress = baseApi;
    client.DefaultRequestHeaders.Add("Accept", "application/json");

}).AddHttpMessageHandler<CookieHandler>();

// Cliente sem cookies para o oEmbed público do Spotify (capas das faixas)
builder.Services.AddHttpClient("Spotify");


await builder.Build().RunAsync();
