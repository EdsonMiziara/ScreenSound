using ScreenSound.Banco;

namespace ScreenSound.Shared.Data.Banco;
public class UnitOfWork : IUnitOfWork
{
    private readonly ScreenSoundContext _context;

    public UnitOfWork(ScreenSoundContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Salva (Commita) todas as mudanças rastreadas pelo DbContext.
    /// </summary>
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
