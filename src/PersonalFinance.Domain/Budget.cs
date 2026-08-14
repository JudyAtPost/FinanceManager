namespace PersonalFinance.Domain;

/// <summary>
/// A spending limit for one category in one specific month.
/// </summary>
public sealed class Budget
{
    private Budget(Guid id, Guid categoryId, BudgetMonth month, decimal limit)
    {
        Id = id;
        CategoryId = categoryId;
        Month = month;
        Limit = limit;
    }

    /// <summary>Required by the persistence layer for materialization.</summary>
    private Budget()
    {
    }

    /// <summary>Gets the identifier of the budget.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the identifier of the budgeted category.</summary>
    public Guid CategoryId { get; private set; }

    /// <summary>Gets the category the budget applies to, when loaded.</summary>
    public Category? Category { get; private set; }

    /// <summary>Gets the month the budget applies to.</summary>
    public BudgetMonth Month { get; private set; }

    /// <summary>Gets the spending limit for the month.</summary>
    public decimal Limit { get; private set; }

    /// <summary>Creates a new budget.</summary>
    /// <param name="categoryId">The category the limit applies to.</param>
    /// <param name="month">The month the limit applies to.</param>
    /// <param name="limit">A positive spending limit.</param>
    /// <returns>The created budget.</returns>
    /// <exception cref="DomainValidationException">A supplied value violates a budget invariant.</exception>
    public static Budget Create(Guid categoryId, BudgetMonth month, decimal limit) =>
        new(Guid.CreateVersion7(), ValidateCategoryId(categoryId), month, ValidateLimit(limit));

    /// <summary>Changes the spending limit of the budget.</summary>
    /// <param name="limit">The new positive spending limit.</param>
    /// <exception cref="DomainValidationException">The limit is not greater than zero.</exception>
    public void ChangeLimit(decimal limit) => Limit = ValidateLimit(limit);

    private static decimal ValidateLimit(decimal limit) =>
        limit > 0m
            ? decimal.Round(limit, 2, MidpointRounding.AwayFromZero)
            : throw new DomainValidationException("Budget limit must be greater than zero.");

    private static Guid ValidateCategoryId(Guid categoryId) =>
        categoryId != Guid.Empty
            ? categoryId
            : throw new DomainValidationException("Budget must be assigned to a category.");
}
