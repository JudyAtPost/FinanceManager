using PersonalFinance.Domain;

namespace PersonalFinance.Application.Budgets;

/// <summary>
/// A budget as returned by the API.
/// </summary>
/// <param name="Id">The identifier of the budget.</param>
/// <param name="CategoryId">The budgeted category.</param>
/// <param name="CategoryName">The display name of the budgeted category.</param>
/// <param name="Year">The calendar year the budget applies to.</param>
/// <param name="Month">The month number the budget applies to.</param>
/// <param name="Limit">The spending limit.</param>
public sealed record BudgetDto(Guid Id, Guid CategoryId, string CategoryName, int Year, int Month, decimal Limit)
{
    /// <summary>Projects a domain budget onto its transport representation.</summary>
    /// <param name="budget">The budget to project; its category must be loaded.</param>
    /// <returns>The transport representation.</returns>
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

/// <summary>
/// Payload used to create a budget for one category and month.
/// </summary>
/// <param name="CategoryId">The category the limit applies to.</param>
/// <param name="Year">The calendar year the limit applies to.</param>
/// <param name="Month">The month number the limit applies to.</param>
/// <param name="Limit">A positive spending limit.</param>
public sealed record CreateBudgetRequest(Guid CategoryId, int Year, int Month, decimal Limit);

/// <summary>
/// Payload used to change the limit of an existing budget.
/// </summary>
/// <param name="Limit">The new positive spending limit.</param>
public sealed record UpdateBudgetRequest(decimal Limit);
