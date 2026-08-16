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

    public static Transaction Create(string description, decimal amount, DateOnly date, Guid categoryId) =>
        new(Guid.CreateVersion7(), NormalizeDescription(description), ValidateAmount(amount), date, ValidateCategoryId(categoryId));

    public void Update(string description, decimal amount, DateOnly date, Guid categoryId)
    {
        Description = NormalizeDescription(description);
        Amount = ValidateAmount(amount);
        Date = date;
        CategoryId = ValidateCategoryId(categoryId);

        if (Category is not null && Category.Id != categoryId)
        {
            Category = null;
        }
    }

    private static string NormalizeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainValidationException("Transaction description must not be empty.");
        }

        string trimmed = description.Trim();
        if (trimmed.Length > MaxDescriptionLength)
        {
            throw new DomainValidationException($"Transaction description must not exceed {MaxDescriptionLength} characters.");
        }

        return trimmed;
    }

    private static decimal ValidateAmount(decimal amount) =>
        amount > 0m
            ? decimal.Round(amount, 2, MidpointRounding.AwayFromZero)
            : throw new DomainValidationException("Transaction amount must be greater than zero.");

    private static Guid ValidateCategoryId(Guid categoryId) =>
        categoryId != Guid.Empty
            ? categoryId
            : throw new DomainValidationException("Transaction must be assigned to a category.");
}
