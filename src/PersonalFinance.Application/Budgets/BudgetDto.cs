using PersonalFinance.Domain;

namespace PersonalFinance.Application.Budgets;

public sealed record BudgetDto(Guid Id, Guid CategoryId, string CategoryName, int Year, int Month, decimal Limit)
{
    public static BudgetDto FromDomain(Budget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);

        return new BudgetDto(
            budget.Id,
            budget.CategoryId,
            budget.Category?.Name ?? string.Empty,
            budget.Month.Year,
            budget.Month.Month,
            budget.Limit);
    }
}

public sealed record CreateBudgetRequest(Guid CategoryId, int Year, int Month, decimal Limit);

public sealed record UpdateBudgetRequest(decimal Limit);
