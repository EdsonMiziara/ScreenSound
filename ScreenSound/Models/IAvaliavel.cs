namespace ScreenSound.Models;

interface IAvaliavel
{
    void AdicionarNota(Avaliacao nota);
    double Media { get; }
}
