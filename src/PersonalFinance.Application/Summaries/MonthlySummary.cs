using PersonalFinance.Application.Budgets;

namespace PersonalFinance.Application.Summaries;

/// <summary>
/// The complete overview of one month: totals, breakdown, budget comparison, and top spending category.
/// </summary>
/// <param name="Year">The calendar year of the summary.</param>
/// <param name="Month">The month number of the summary.</param>
/// <param name="TotalIncome">Everything earned in the month.</param>
/// <param name="TotalExpenses">Everything spent in the month.</param>
/// <param name="Breakdown">Per-category totals, largest first within income and expenses.</param>
/// <param name="Budgets">Budget versus actual comparison for every budgeted category.</param>
/// <param name="TopExpenseCategory">The expense category with the highest total, if any expense exists.</param>
public sealed record MonthlySummary(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpenses,
    IReadOnlyList<CategoryBreakdownItem> Breakdown,
    IReadOnlyList<BudgetComparison> Budgets,
    CategoryBreakdownItem? TopExpenseCategory)
{
    /// <summary>Gets what is left of the month's income after all expenses.</summary>
    public decimal Balance => TotalIncome - TotalExpenses;
}
