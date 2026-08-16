using PersonalFinance.Domain;

namespace PersonalFinance.Application.Abstractions;

public sealed record CategoryTotal(Guid CategoryId, string CategoryName, TransactionType Type, decimal Total);
