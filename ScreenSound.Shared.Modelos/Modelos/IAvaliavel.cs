namespace ScreenSound.Modelos;

interface IAvaliavel
{
    void AdicionarNota(int pessoaId, int nota);
    double Media { get; }
}