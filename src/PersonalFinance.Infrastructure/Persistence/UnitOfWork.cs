using PersonalFinance.Application.Abstractions;

namespace PersonalFinance.Infrastructure.Persistence;

/// <summary>
/// Commits pending EF Core changes.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly FinanceDbContext _context;

    /// <summary>Initializes a new instance of the <see cref="UnitOfWork"/> class.</summary>
    /// <param name="context">The database context.</param>
    public UnitOfWork(FinanceDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        _context.SaveChangesAsync(cancellationToken);
}
