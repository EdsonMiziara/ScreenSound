namespace ScreenSound.Shared.Data.Banco;

public interface IUnitOfWork
{
    /// <summary>
    /// Salva (Commita) todas as mudanças rastreadas pelo DbContext para o banco de dados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>O número de linhas afetadas.</returns>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
