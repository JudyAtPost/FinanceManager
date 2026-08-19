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

        if (transaction.Category is null || transaction.Category.Id != transaction.CategoryId)
        {
            throw new InvalidOperationException(
                $"The category of transaction '{transaction.Id}' has not been loaded; the query is missing an include.");
        }

        return new TransactionDto(
            transaction.Id,
            transaction.Description,
            transaction.Amount,
            transaction.Date,
            transaction.CategoryId,
            transaction.Category.Name,
            transaction.Category.Type);
    }
}

public sealed record SaveTransactionRequest(string Description, decimal Amount, DateOnly Date, Guid CategoryId);
