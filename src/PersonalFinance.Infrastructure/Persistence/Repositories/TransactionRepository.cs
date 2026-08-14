using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Common;
using PersonalFinance.Application.Transactions;
using PersonalFinance.Domain;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core backed <see cref="ITransactionRepository"/> with server-side filtering and paging.
/// </summary>
public sealed class TransactionRepository : ITransactionRepository
{
    private readonly FinanceDbContext _context;

    /// <summary>Initializes a new instance of the <see cref="TransactionRepository"/> class.</summary>
    /// <param name="context">The database context.</param>
    public TransactionRepository(FinanceDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task<PagedResult<Transaction>> ListAsync(TransactionQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryable<Transaction> filtered = _context.Transactions
            .AsNoTracking()
            .Include(transaction => transaction.Category)
            .AsQueryable();

        if (query.Month is { } month)
        {
            DateOnly first = month.FirstDay;
            DateOnly last = month.LastDay;
            filtered = filtered.Where(transaction => transaction.Date >= first && transaction.Date <= last);
        }

        if (query.CategoryId is { } categoryId)
        {
            filtered = filtered.Where(transaction => transaction.CategoryId == categoryId);
        }

        if (query.Type is { } type)
        {
            filtered = filtered.Where(transaction => transaction.Category!.Type == type);
        }

        int totalCount = await filtered.CountAsync(cancellationToken).ConfigureAwait(false);

        List<Transaction> items = await filtered
            .OrderByDescending(transaction => transaction.Date)
            .ThenByDescending(transaction => transaction.Id)
            .Skip(query.Skip)
            .Take(query.NormalizedPageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<Transaction>(items, totalCount, query.NormalizedPage, query.NormalizedPageSize);
    }

    /// <inheritdoc />
    public async Task<Transaction?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.Transactions
            .Include(transaction => transaction.Category)
            .FirstOrDefaultAsync(transaction => transaction.Id == id, cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void Add(Transaction transaction) => _context.Transactions.Add(transaction);

    /// <inheritdoc />
    public void Remove(Transaction transaction) => _context.Transactions.Remove(transaction);
}
