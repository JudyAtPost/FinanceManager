using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Budgets;
using PersonalFinance.Application.Common;
using PersonalFinance.Domain;

namespace PersonalFinance.Application.Summaries;

public sealed class MonthlySummaryService
{
    private readonly ITransactionRepository _transactions;
    private readonly IBudgetRepository _budgets;

    public MonthlySummaryService(ITransactionRepository transactions, IBudgetRepository budgets)
    {
        _transactions = transactions;
        _budgets = budgets;
    }

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
            BudgetComparisonCalculator.Compare(budgets, totals),
            topExpense);
    }

    private static decimal Share(decimal value, decimal total) => Money.Percentage(value, total);
}
