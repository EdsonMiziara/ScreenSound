namespace ScreenSound.Shared.Modelos.Modelos;

interface IAvaliavel<T>
{
    void AdicionarNota(int pessoaId, int nota);
    double Media { get; }
}