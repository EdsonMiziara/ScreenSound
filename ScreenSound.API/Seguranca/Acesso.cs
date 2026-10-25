namespace ScreenSound.API.Seguranca;

public static class Perfis
{
    public const string Administrador = "Administrador";
}

public static class Politicas
{
    /// <summary>Gerenciamento do catálogo (artistas, faixas, gêneros, álbuns) e dos usuários.</summary>
    public const string Admin = "Admin";
}

public class ContaDemo
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string? Nome { get; set; }
    public bool Admin { get; set; }
}

public class OpcoesAcesso
{
    /// <summary>E-mails que sempre recebem o perfil de administrador ao iniciar a API.</summary>
    public List<string> Administradores { get; set; } = new();

    /// <summary>Contas públicas de demonstração, criadas automaticamente se não existirem.</summary>
    public List<ContaDemo> ContasDemo { get; set; } = new();

    public bool EhContaDemo(string? email) =>
        email is not null && ContasDemo.Any(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
}
