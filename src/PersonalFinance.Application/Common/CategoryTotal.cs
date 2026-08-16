using PersonalFinance.Domain;

namespace PersonalFinance.Application.Common;

public sealed record CategoryTotal(Guid CategoryId, string CategoryName, TransactionType Type, decimal Total);
