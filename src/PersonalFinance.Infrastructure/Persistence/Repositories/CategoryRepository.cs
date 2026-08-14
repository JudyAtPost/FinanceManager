using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Domain;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core backed <see cref="ICategoryRepository"/>.
/// </summary>
public sealed class CategoryRepository : ICategoryRepository
{
    private readonly FinanceDbContext _context;

    /// <summary>Initializes a new instance of the <see cref="CategoryRepository"/> class.</summary>
    /// <param name="context">The database context.</param>
    public CategoryRepository(FinanceDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task<IReadOnlyList<Category>> ListAsync(CancellationToken cancellationToken) =>
        await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<Category?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.Categories
            .FirstOrDefaultAsync(category => category.Id == id, cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.Categories
            .AnyAsync(category => category.Id == id, cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<bool> IsInUseAsync(Guid id, CancellationToken cancellationToken)
    {
        bool usedByTransactions = await _context.Transactions
            .AnyAsync(transaction => transaction.CategoryId == id, cancellationToken)
            .ConfigureAwait(false);

        return usedByTransactions
            || await _context.Budgets
                .AnyAsync(budget => budget.CategoryId == id, cancellationToken)
                .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Add(Category category) => _context.Categories.Add(category);

    /// <inheritdoc />
    public void Remove(Category category) => _context.Categories.Remove(category);
}
