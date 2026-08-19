using PersonalFinance.Application.Common;
using PersonalFinance.Domain;

namespace PersonalFinance.Application.Budgets;

public static class BudgetComparisonCalculator
{
    /// <summary>Compares each budget against the expense total of its category.</summary>
    public static IReadOnlyList<BudgetComparison> Compare(
        IReadOnlyList<Budget> budgets,
        IReadOnlyList<CategoryTotal> totals)
    {
        ArgumentNullException.ThrowIfNull(budgets);
        ArgumentNullException.ThrowIfNull(totals);

        if (budgets.Count == 0)
        {
            return [];
        }

        Dictionary<Guid, decimal> spentByCategory = [];
        Dictionary<Guid, string> namesByCategory = [];

        foreach (CategoryTotal total in totals)
        {
            namesByCategory[total.CategoryId] = total.CategoryName;

            if (total.Type == TransactionType.Expense)
            {
                spentByCategory[total.CategoryId] = spentByCategory.GetValueOrDefault(total.CategoryId) + total.Total;
            }
        }

        return
        [
            .. budgets
                .Select(budget => new BudgetComparison(
                    budget.CategoryId,
                    budget.Category?.Name ?? namesByCategory.GetValueOrDefault(budget.CategoryId, string.Empty),
                    budget.Limit,
                    spentByCategory.GetValueOrDefault(budget.CategoryId)))
                .OrderByDescending(comparison => comparison.OverspentBy)
                .ThenByDescending(comparison => comparison.Spent)
                .ThenBy(comparison => comparison.CategoryName, StringComparer.OrdinalIgnoreCase)
        ];
    }
}
