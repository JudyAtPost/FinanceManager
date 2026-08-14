namespace PersonalFinance.Application.Abstractions;

/// <summary>
/// Commits pending changes made through the repositories.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Persists all pending changes.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The number of affected rows.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
