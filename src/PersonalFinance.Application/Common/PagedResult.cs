namespace PersonalFinance.Application.Common;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    /// <summary>Gets the total number of pages available for the query.</summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Projects each item onto a new shape while keeping the paging metadata.</summary>
    public PagedResult<TResult> Map<TResult>(Func<T, TResult> selector) =>
        new([.. Items.Select(selector)], TotalCount, Page, PageSize);
}
