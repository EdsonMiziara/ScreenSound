namespace ScreenSound.API.Requests;

public record PerfilRequest(string? NomeExibicao, string? Bio);

public record FotoPerfilRequest(string FotoBase64);

public record TrocaSenhaRequest(string SenhaAtual, string NovaSenha);

public record AlterarAdminRequest(bool Admin);
