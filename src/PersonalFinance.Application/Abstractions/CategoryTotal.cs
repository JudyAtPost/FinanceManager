using PersonalFinance.Domain;

namespace PersonalFinance.Application.Abstractions;

/// <summary>
/// The summed amount of all transactions of one category within a month.
/// </summary>
/// <param name="CategoryId">The identifier of the category.</param>
/// <param name="CategoryName">The display name of the category.</param>
/// <param name="Type">Whether the category groups income or expenses.</param>
/// <param name="Total">The summed amount, always positive.</param>
public sealed record CategoryTotal(Guid CategoryId, string CategoryName, TransactionType Type, decimal Total);
