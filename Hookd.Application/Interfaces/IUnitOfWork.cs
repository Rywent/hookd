namespace Hookd.Application.Interfaces;

// Saves pending changes from the current operation.
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
