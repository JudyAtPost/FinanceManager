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

    public static Result<Budget> Create(Guid categoryId, BudgetMonth month, decimal limit) =>
        ValidateCategoryId(categoryId)
            .Bind(() => ValidateLimit(limit))
            .Map(validLimit => new Budget(Guid.CreateVersion7(), categoryId, month, validLimit));

    public Result ChangeLimit(decimal limit) =>
        ValidateLimit(limit).Bind(validLimit =>
        {
            Limit = validLimit;
            return Result.Success();
        });

    private static Result<decimal> ValidateLimit(decimal limit) =>
        limit > 0m
            ? decimal.Round(limit, 2, MidpointRounding.AwayFromZero)
            : Error.Validation("Budget limit must be greater than zero.");

    private static Result ValidateCategoryId(Guid categoryId) =>
        categoryId != Guid.Empty
            ? Result.Success()
            : Error.Validation("Budget must be assigned to a category.");
}
