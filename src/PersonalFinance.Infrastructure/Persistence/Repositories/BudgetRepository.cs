using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Domain;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core backed <see cref="IBudgetRepository"/>.
/// </summary>
public sealed class BudgetRepository : IBudgetRepository
{
    private readonly FinanceDbContext _context;

    /// <summary>Initializes a new instance of the <see cref="BudgetRepository"/> class.</summary>
    /// <param name="context">The database context.</param>
    public BudgetRepository(FinanceDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task<IReadOnlyList<Budget>> ListForMonthAsync(BudgetMonth month, CancellationToken cancellationToken) =>
        await _context.Budgets
            .AsNoTracking()
            .Include(budget => budget.Category)
            .Where(budget => budget.Month == month)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<Budget?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.Budgets
            .Include(budget => budget.Category)
            .FirstOrDefaultAsync(budget => budget.Id == id, cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<Budget?> GetForCategoryAndMonthAsync(
        Guid categoryId,
        BudgetMonth month,
        CancellationToken cancellationToken) =>
        await _context.Budgets
            .Include(budget => budget.Category)
            .FirstOrDefaultAsync(
                budget => budget.CategoryId == categoryId && budget.Month == month,
                cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public void Add(Budget budget) => _context.Budgets.Add(budget);

    /// <inheritdoc />
    public void Remove(Budget budget) => _context.Budgets.Remove(budget);
}
