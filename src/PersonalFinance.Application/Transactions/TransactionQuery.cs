using PersonalFinance.Domain;

namespace PersonalFinance.Application.Transactions;

public sealed record TransactionQuery
{
    public const int DefaultPageSize = 50;

    public const int MaxPageSize = 200;

    private TransactionQuery(BudgetMonth? month, Guid? categoryId, TransactionType? type, int page, int pageSize)
    {
        Month = month;
        CategoryId = categoryId;
        Type = type;
        Page = page;
        PageSize = pageSize;
    }

    public BudgetMonth? Month { get; }

    public Guid? CategoryId { get; }

    public TransactionType? Type { get; }

    public int Page { get; }

    public int PageSize { get; }

    public int Skip => (Page - 1) * PageSize;

    public static Result<TransactionQuery> Create(
        BudgetMonth? month = null,
        Guid? categoryId = null,
        TransactionType? type = null,
        int? page = null,
        int? pageSize = null)
    {
        int resolvedPage = page ?? 1;
        if (resolvedPage < 1)
        {
            return Error.Validation($"Page must be 1 or greater but was {resolvedPage}.");
        }

        int resolvedPageSize = pageSize ?? DefaultPageSize;
        if (resolvedPageSize < 1 || resolvedPageSize > MaxPageSize)
        {
            return Error.Validation($"Page size must be between 1 and {MaxPageSize} but was {resolvedPageSize}.");
        }

        if (categoryId == Guid.Empty)
        {
            return Error.Validation("Category filter must not be an empty identifier.");
        }

        if (type is not null && !Enum.IsDefined(type.Value))
        {
            return Error.Validation($"Unknown transaction type '{type}'.");
        }

        return new TransactionQuery(month, categoryId, type, resolvedPage, resolvedPageSize);
    }
}
