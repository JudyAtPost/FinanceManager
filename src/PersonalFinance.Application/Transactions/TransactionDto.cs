using PersonalFinance.Domain;

namespace PersonalFinance.Application.Transactions;

/// <summary>
/// A transaction as returned by the API.
/// </summary>
/// <param name="Id">The identifier of the transaction.</param>
/// <param name="Description">The free-text description.</param>
/// <param name="Amount">The positive amount.</param>
/// <param name="Date">The date the transaction occurred.</param>
/// <param name="CategoryId">The identifier of the assigned category.</param>
/// <param name="CategoryName">The display name of the assigned category.</param>
/// <param name="Type">Whether the transaction is income or an expense.</param>
public sealed record TransactionDto(
    Guid Id,
    string Description,
    decimal Amount,
    DateOnly Date,
    Guid CategoryId,
    string CategoryName,
    TransactionType Type)
{
    /// <summary>Projects a domain transaction onto its transport representation.</summary>
    /// <param name="transaction">The transaction to project; its category must be loaded.</param>
    /// <returns>The transport representation.</returns>
    public static TransactionDto FromDomain(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        return new TransactionDto(
            transaction.Id,
            transaction.Description,
            transaction.Amount,
            transaction.Date,
            transaction.CategoryId,
            transaction.Category?.Name ?? string.Empty,
            transaction.Type);
    }
}

/// <summary>
/// Payload used to create or update a transaction.
/// </summary>
/// <param name="Description">The free-text description; must not be blank.</param>
/// <param name="Amount">A positive amount.</param>
/// <param name="Date">The date the transaction occurred.</param>
/// <param name="CategoryId">The category the transaction is assigned to.</param>
public sealed record SaveTransactionRequest(string Description, decimal Amount, DateOnly Date, Guid CategoryId);
