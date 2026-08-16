using PersonalFinance.Domain;

namespace PersonalFinance.Application.Summaries;

public sealed record CategoryBreakdownItem(
    Guid CategoryId,
    string CategoryName,
    TransactionType Type,
    decimal Total,
    decimal ShareOfTypeTotal);
