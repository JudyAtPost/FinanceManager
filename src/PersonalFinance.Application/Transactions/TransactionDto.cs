using PersonalFinance.Domain;

namespace PersonalFinance.Application.Transactions;

public sealed record TransactionDto(
    Guid Id,
    string Description,
    decimal Amount,
    DateOnly Date,
    Guid CategoryId,
    string CategoryName,
    TransactionType Type)
{
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

public sealed record SaveTransactionRequest(string Description, decimal Amount, DateOnly Date, Guid CategoryId);
