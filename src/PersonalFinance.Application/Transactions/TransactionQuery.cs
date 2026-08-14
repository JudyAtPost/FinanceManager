using PersonalFinance.Domain;

namespace PersonalFinance.Application.Transactions;

/// <summary>
/// Server-side filter and paging options for listing transactions.
/// </summary>
/// <param name="Month">Restricts the result to a single month; <see langword="null"/> means all months.</param>
/// <param name="CategoryId">Restricts the result to a single category; <see langword="null"/> means all categories.</param>
/// <param name="Type">Restricts the result to income or expenses; <see langword="null"/> means both.</param>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Number of items per page.</param>
public sealed record TransactionQuery(
    BudgetMonth? Month = null,
    Guid? CategoryId = null,
    TransactionType? Type = null,
    int Page = 1,
    int PageSize = 50)
{
    /// <summary>Largest page size the API will serve.</summary>
    public const int MaxPageSize = 200;

    /// <summary>Gets the sanitized one-based page number.</summary>
    public int NormalizedPage => Page < 1 ? 1 : Page;

    /// <summary>Gets the sanitized page size, clamped to <see cref="MaxPageSize"/>.</summary>
    public int NormalizedPageSize => Math.Clamp(PageSize < 1 ? 50 : PageSize, 1, MaxPageSize);

    /// <summary>Gets the number of items to skip for the requested page.</summary>
    public int Skip => (NormalizedPage - 1) * NormalizedPageSize;
}
