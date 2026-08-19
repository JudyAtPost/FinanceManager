using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Common;
using PersonalFinance.Application.Transactions;
using PersonalFinance.Domain;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly FinanceDbContext _context;

    public TransactionRepository(FinanceDbContext context) => _context = context;

    public async Task<PagedResult<Transaction>> ListAsync(TransactionQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryable<Transaction> filtered = _context.Transactions
            .AsNoTracking()
            .Include(transaction => transaction.Category)
            .Where(transaction => query.Month == null
                || (transaction.Date >= query.Month.Value.FirstDay && transaction.Date <= query.Month.Value.LastDay))
            .Where(transaction => query.CategoryId == null || transaction.CategoryId == query.CategoryId)
            .Where(transaction => query.Type == null || transaction.Category!.Type == query.Type);

        int totalCount = await filtered.CountAsync(cancellationToken).ConfigureAwait(false);

        List<Transaction> items = await filtered
            .OrderByDescending(transaction => transaction.Date)
            .ThenByDescending(transaction => transaction.Id)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<Transaction>(items, totalCount, query.Page, query.PageSize);
    }

    public async Task<Transaction?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.Transactions
            .Include(transaction => transaction.Category)
            .FirstOrDefaultAsync(transaction => transaction.Id == id, cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<CategoryTotal>> GetMonthlyTotalsByCategoryAsync(
        BudgetMonth month,
        CancellationToken cancellationToken)
    {
        DateOnly first = month.FirstDay;
        DateOnly last = month.LastDay;

        return await _context.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.Date >= first && transaction.Date <= last)
            .GroupBy(transaction => new { transaction.CategoryId, transaction.Category!.Name, transaction.Category!.Type })
            .Select(group => new CategoryTotal(
                group.Key.CategoryId,
                group.Key.Name,
                group.Key.Type,
                group.Sum(transaction => transaction.Amount)))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public void Add(Transaction transaction) => _context.Transactions.Add(transaction);

    public void Remove(Transaction transaction) => _context.Transactions.Remove(transaction);
}
