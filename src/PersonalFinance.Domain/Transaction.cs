namespace PersonalFinance.Domain;

public sealed class Transaction
{
    public const int MaxDescriptionLength = 200;

    private Transaction(Guid id, string description, decimal amount, DateOnly date, Guid categoryId)
    {
        Id = id;
        Description = description;
        Amount = amount;
        Date = date;
        CategoryId = categoryId;
    }

    private Transaction()
    {
        Description = string.Empty;
    }

    public Guid Id { get; private set; }

    public string Description { get; private set; }

    public decimal Amount { get; private set; }

    public DateOnly Date { get; private set; }

    public Guid CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public TransactionType Type => Category?.Type
        ?? throw new InvalidOperationException("The category of the transaction has not been loaded.");

    public static Result<Transaction> Create(string description, decimal amount, DateOnly date, Guid categoryId) =>
        Validate(description, amount, categoryId)
            .Map(valid => new Transaction(Guid.CreateVersion7(), valid.Description, valid.Amount, date, categoryId));

    public Result Update(string description, decimal amount, DateOnly date, Guid categoryId) =>
        Validate(description, amount, categoryId).Bind(valid =>
        {
            Description = valid.Description;
            Amount = valid.Amount;
            Date = date;
            CategoryId = categoryId;

            return Result.Success();
        });

    private static Result<(string Description, decimal Amount)> Validate(string description, decimal amount, Guid categoryId) =>
        NormalizeDescription(description)
            .Bind(validDescription => ValidateAmount(amount).Map(validAmount => (validDescription, validAmount)))
            .Ensure(_ => categoryId != Guid.Empty, Error.Validation("Transaction must be assigned to a category."));

    private static Result<string> NormalizeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Error.Validation("Transaction description must not be empty.");
        }

        string trimmed = description.Trim();
        if (trimmed.Length > MaxDescriptionLength)
        {
            return Error.Validation($"Transaction description must not exceed {MaxDescriptionLength} characters.");
        }

        return trimmed;
    }

    private static Result<decimal> ValidateAmount(decimal amount) =>
        amount > 0m
            ? Money.Round(amount)
            : Error.Validation("Transaction amount must be greater than zero.");
}
