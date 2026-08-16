using PersonalFinance.Domain;

namespace PersonalFinance.Application.Transactions;

public sealed record TransactionQuery(
    BudgetMonth? Month = null,
    Guid? CategoryId = null,
    TransactionType? Type = null,
    int Page = 1,
    int PageSize = 50)
{
    public const int MaxPageSize = 200;

    public int NormalizedPage => Page < 1 ? 1 : Page;

    public int NormalizedPageSize => Math.Clamp(PageSize < 1 ? 50 : PageSize, 1, MaxPageSize);

    public int Skip => (NormalizedPage - 1) * NormalizedPageSize;
}
