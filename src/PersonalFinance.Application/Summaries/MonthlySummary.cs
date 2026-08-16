using PersonalFinance.Application.Budgets;

namespace PersonalFinance.Application.Summaries;

public sealed record MonthlySummary(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpenses,
    IReadOnlyList<CategoryBreakdownItem> Breakdown,
    IReadOnlyList<BudgetComparison> Budgets,
    CategoryBreakdownItem? TopExpenseCategory)
{
    public decimal Balance => TotalIncome - TotalExpenses;
}
