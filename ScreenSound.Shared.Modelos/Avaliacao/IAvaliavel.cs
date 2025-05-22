namespace ScreenSound.Shared.Modelos.Avaliacao;

interface IAvaliavel<T>
{
    void AdicionarNota(int pessoaId, int nota);
    double Media { get; }
}