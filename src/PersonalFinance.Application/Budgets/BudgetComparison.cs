namespace PersonalFinance.Application.Budgets;

public sealed record BudgetComparison(Guid CategoryId, string CategoryName, decimal Limit, decimal Spent)
{
    public decimal Remaining => Math.Max(0m, Limit - Spent);

    public decimal OverspentBy => Math.Max(0m, Spent - Limit);

    public bool IsOverBudget => Spent > Limit;

    public decimal UsagePercentage => Limit <= 0m ? 0m : decimal.Round(Spent / Limit * 100m, 2, MidpointRounding.AwayFromZero);
}
