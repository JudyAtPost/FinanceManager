using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Domain;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly FinanceDbContext _context;

    public CategoryRepository(FinanceDbContext context) => _context = context;

    public async Task<IReadOnlyList<Category>> ListAsync(CancellationToken cancellationToken) =>
        await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<Category?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.Categories
            .FirstOrDefaultAsync(category => category.Id == id, cancellationToken)
            .ConfigureAwait(false);

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

    public void Add(Category category) => _context.Categories.Add(category);

    public void Remove(Category category) => _context.Categories.Remove(category);
}
