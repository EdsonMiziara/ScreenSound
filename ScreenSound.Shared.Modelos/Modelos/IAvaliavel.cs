namespace ScreenSound.Modelos;

interface IAvaliavel
{
    void AdicionarNota(Avaliacao nota);
    double Media { get; }
}