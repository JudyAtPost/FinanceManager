using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Budgets;
using PersonalFinance.Domain;

namespace PersonalFinance.Application.Summaries;

/// <summary>
/// Builds the overview of a single month from the recorded transactions and budgets.
/// </summary>
public sealed class MonthlySummaryService
{
    private readonly ITransactionRepository _transactions;
    private readonly IBudgetRepository _budgets;

    /// <summary>Initializes a new instance of the <see cref="MonthlySummaryService"/> class.</summary>
    /// <param name="transactions">Transaction storage, used for the aggregated totals.</param>
    /// <param name="budgets">Budget storage, used for the budget versus actual comparison.</param>
    public MonthlySummaryService(ITransactionRepository transactions, IBudgetRepository budgets)
    {
        _transactions = transactions;
        _budgets = budgets;
    }

    /// <summary>Builds the summary of the supplied month.</summary>
    /// <param name="month">The month to summarize.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The summary of the month.</returns>
    public async Task<MonthlySummary> GetAsync(BudgetMonth month, CancellationToken cancellationToken)
    {
        IReadOnlyList<CategoryTotal> totals = await _transactions
            .GetMonthlyTotalsByCategoryAsync(month, cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyList<Budget> budgets = await _budgets
            .ListForMonthAsync(month, cancellationToken)
            .ConfigureAwait(false);

        return Build(month, totals, budgets);
    }

    /// <summary>Builds the summary from already aggregated data, without touching storage.</summary>
    /// <param name="month">The month being summarized.</param>
    /// <param name="totals">Per-category totals of the month.</param>
    /// <param name="budgets">Budgets defined for the month.</param>
    /// <returns>The summary of the month.</returns>
    public static MonthlySummary Build(
        BudgetMonth month,
        IReadOnlyList<CategoryTotal> totals,
        IReadOnlyList<Budget> budgets)
    {
        ArgumentNullException.ThrowIfNull(totals);
        ArgumentNullException.ThrowIfNull(budgets);

        decimal totalIncome = totals.Where(total => total.Type == TransactionType.Income).Sum(total => total.Total);
        decimal totalExpenses = totals.Where(total => total.Type == TransactionType.Expense).Sum(total => total.Total);

        List<CategoryBreakdownItem> breakdown =
        [
            .. totals
                .Select(total => new CategoryBreakdownItem(
                    total.CategoryId,
                    total.CategoryName,
                    total.Type,
                    total.Total,
                    Share(total.Total, total.Type == TransactionType.Income ? totalIncome : totalExpenses)))
                .OrderBy(item => item.Type)
                .ThenByDescending(item => item.Total)
                .ThenBy(item => item.CategoryName, StringComparer.OrdinalIgnoreCase)
        ];

        CategoryBreakdownItem? topExpense = breakdown
            .Where(item => item.Type == TransactionType.Expense)
            .MaxBy(item => item.Total);

        return new MonthlySummary(
            month.Year,
            month.Month,
            totalIncome,
            totalExpenses,
            breakdown,
            BudgetService.Compare(budgets, totals),
            topExpense);
    }

    private static decimal Share(decimal value, decimal total) =>
        total <= 0m ? 0m : decimal.Round(value / total * 100m, 2, MidpointRounding.AwayFromZero);
}
