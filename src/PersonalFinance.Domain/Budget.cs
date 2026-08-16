namespace PersonalFinance.Domain;

public sealed class Budget
{
    private Budget(Guid id, Guid categoryId, BudgetMonth month, decimal limit)
    {
        Id = id;
        CategoryId = categoryId;
        Month = month;
        Limit = limit;
    }

    private Budget()
    {
    }

    public Guid Id { get; private set; }

    public Guid CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public BudgetMonth Month { get; private set; }

    public decimal Limit { get; private set; }

    public static Budget Create(Guid categoryId, BudgetMonth month, decimal limit) =>
        new(Guid.CreateVersion7(), ValidateCategoryId(categoryId), month, ValidateLimit(limit));

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
