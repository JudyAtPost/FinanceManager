namespace PersonalFinance.Application.Budgets;

/// <summary>
/// The comparison of a category budget against what was actually spent in that month.
/// </summary>
/// <param name="CategoryId">The identifier of the budgeted category.</param>
/// <param name="CategoryName">The display name of the budgeted category.</param>
/// <param name="Limit">The budgeted spending limit.</param>
/// <param name="Spent">The amount actually spent in the month.</param>
public sealed record BudgetComparison(Guid CategoryId, string CategoryName, decimal Limit, decimal Spent)
{
    /// <summary>Gets the amount still available; zero once the budget is exhausted.</summary>
    public decimal Remaining => Math.Max(0m, Limit - Spent);

    /// <summary>Gets the amount spent beyond the limit; zero while inside the budget.</summary>
    public decimal OverspentBy => Math.Max(0m, Spent - Limit);

    /// <summary>Gets a value indicating whether the budget was exceeded.</summary>
    public bool IsOverBudget => Spent > Limit;

    /// <summary>Gets how much of the budget was used, in percent, rounded to two decimals.</summary>
    public decimal UsagePercentage => Limit <= 0m ? 0m : decimal.Round(Spent / Limit * 100m, 2, MidpointRounding.AwayFromZero);
}
