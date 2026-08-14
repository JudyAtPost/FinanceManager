using PersonalFinance.Application.Common;
using PersonalFinance.Application.Transactions;
using PersonalFinance.Domain;

namespace PersonalFinance.Application.Abstractions;

/// <summary>
/// Read and write access to <see cref="Transaction"/> aggregates.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>Lists transactions matching the supplied filter, newest first.</summary>
    /// <param name="query">The filter and paging options.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A page of matching transactions with their categories loaded.</returns>
    Task<PagedResult<Transaction>> ListAsync(TransactionQuery query, CancellationToken cancellationToken);

    /// <summary>Loads a single transaction including its category.</summary>
    /// <param name="id">The identifier of the transaction.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The transaction, or <see langword="null"/> when it does not exist.</returns>
    Task<Transaction?> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Aggregates the totals per category for one month.</summary>
    /// <param name="month">The month to aggregate.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>One entry per category that has at least one transaction in the month.</returns>
    Task<IReadOnlyList<CategoryTotal>> GetMonthlyTotalsByCategoryAsync(BudgetMonth month, CancellationToken cancellationToken);

    /// <summary>Adds a new transaction.</summary>
    /// <param name="transaction">The transaction to add.</param>
    void Add(Transaction transaction);

    /// <summary>Removes an existing transaction.</summary>
    /// <param name="transaction">The transaction to remove.</param>
    void Remove(Transaction transaction);
}
