using System.ComponentModel.DataAnnotations.Schema;

namespace ScreenSound.Shared.Modelos.Validacao;

public abstract class Valida : IValidavel
{
    private readonly Erros erros = new();
    public bool EhValido => erros.Count() == 0;
    [NotMapped]
    public Erros Erros => erros;
    protected abstract void Validar();

}

