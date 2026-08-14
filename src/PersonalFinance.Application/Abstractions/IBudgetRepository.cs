using PersonalFinance.Domain;

namespace PersonalFinance.Application.Abstractions;

/// <summary>
/// Read and write access to <see cref="Budget"/> aggregates.
/// </summary>
public interface IBudgetRepository
{
    /// <summary>Lists all budgets defined for one month, with their categories loaded.</summary>
    /// <param name="month">The month to list budgets for.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>All budgets of the month.</returns>
    Task<IReadOnlyList<Budget>> ListForMonthAsync(BudgetMonth month, CancellationToken cancellationToken);

    /// <summary>Loads a single budget including its category.</summary>
    /// <param name="id">The identifier of the budget.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The budget, or <see langword="null"/> when it does not exist.</returns>
    Task<Budget?> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Loads the budget defined for one category in one month.</summary>
    /// <param name="categoryId">The identifier of the category.</param>
    /// <param name="month">The month the budget applies to.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The budget, or <see langword="null"/> when no budget is defined.</returns>
    Task<Budget?> GetForCategoryAndMonthAsync(Guid categoryId, BudgetMonth month, CancellationToken cancellationToken);

    /// <summary>Adds a new budget.</summary>
    /// <param name="budget">The budget to add.</param>
    void Add(Budget budget);

    /// <summary>Removes an existing budget.</summary>
    /// <param name="budget">The budget to remove.</param>
    void Remove(Budget budget);
}
