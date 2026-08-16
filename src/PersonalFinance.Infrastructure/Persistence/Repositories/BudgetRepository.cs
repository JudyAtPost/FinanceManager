using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Domain;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public sealed class BudgetRepository : IBudgetRepository
{
    private readonly FinanceDbContext _context;

    public BudgetRepository(FinanceDbContext context) => _context = context;

    public async Task<IReadOnlyList<Budget>> ListForMonthAsync(BudgetMonth month, CancellationToken cancellationToken) =>
        await _context.Budgets
            .AsNoTracking()
            .Include(budget => budget.Category)
            .Where(budget => budget.Month == month)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<Budget?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.Budgets
            .Include(budget => budget.Category)
            .FirstOrDefaultAsync(budget => budget.Id == id, cancellationToken)
            .ConfigureAwait(false);

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

    public void Add(Budget budget) => _context.Budgets.Add(budget);

    public void Remove(Budget budget) => _context.Budgets.Remove(budget);
}
