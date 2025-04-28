using Microsoft.AspNetCore.Mvc;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using System.Runtime.CompilerServices;

namespace ScreenSound.API.Requests;

public record class GeneroRequest(string Nome, string? DescricaoGenero = null);
