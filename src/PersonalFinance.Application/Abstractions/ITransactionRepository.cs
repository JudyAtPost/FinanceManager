using PersonalFinance.Application.Common;
using PersonalFinance.Application.Transactions;
using PersonalFinance.Domain;

namespace PersonalFinance.Application.Abstractions;

public interface ITransactionRepository
{
    Task<PagedResult<Transaction>> ListAsync(TransactionQuery query, CancellationToken cancellationToken);

    Task<Transaction?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<CategoryTotal>> GetMonthlyTotalsByCategoryAsync(BudgetMonth month, CancellationToken cancellationToken);

    void Add(Transaction transaction);

    void Remove(Transaction transaction);
}
