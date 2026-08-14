using PersonalFinance.Domain;

namespace PersonalFinance.Application.Summaries;

/// <summary>
/// One line of the per-category breakdown of a month.
/// </summary>
/// <param name="CategoryId">The identifier of the category.</param>
/// <param name="CategoryName">The display name of the category.</param>
/// <param name="Type">Whether the category groups income or expenses.</param>
/// <param name="Total">The summed amount booked on the category in the month.</param>
/// <param name="ShareOfTypeTotal">The share of the month's income or expense total, in percent.</param>
public sealed record CategoryBreakdownItem(
    Guid CategoryId,
    string CategoryName,
    TransactionType Type,
    decimal Total,
    decimal ShareOfTypeTotal);
