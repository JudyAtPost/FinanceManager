namespace PersonalFinance.Domain;

/// <summary>
/// A single recorded income or expense, assigned to a category and dated for time-based evaluation.
/// </summary>
public sealed class Transaction
{
    /// <summary>Maximum number of characters allowed in <see cref="Description"/>.</summary>
    public const int MaxDescriptionLength = 200;

    private Transaction(Guid id, string description, decimal amount, DateOnly date, Guid categoryId)
    {
        Id = id;
        Description = description;
        Amount = amount;
        Date = date;
        CategoryId = categoryId;
    }

    /// <summary>Required by the persistence layer for materialization.</summary>
    private Transaction()
    {
        Description = string.Empty;
    }

    /// <summary>Gets the identifier of the transaction.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the free-text description, for example "Lunch at a café".</summary>
    public string Description { get; private set; }

    /// <summary>Gets the amount, always stored as a positive value; direction comes from the category type.</summary>
    public decimal Amount { get; private set; }

    /// <summary>Gets the date the transaction occurred.</summary>
    public DateOnly Date { get; private set; }

    /// <summary>Gets the identifier of the category the transaction belongs to.</summary>
    public Guid CategoryId { get; private set; }

    /// <summary>Gets the category the transaction belongs to, when loaded.</summary>
    public Category? Category { get; private set; }

    /// <summary>Gets the kind of transaction, derived from the assigned category.</summary>
    /// <exception cref="InvalidOperationException">The category navigation has not been loaded.</exception>
    public TransactionType Type => Category?.Type
        ?? throw new InvalidOperationException("The category of the transaction has not been loaded.");

    /// <summary>Creates a new transaction.</summary>
    /// <param name="description">Free-text description; must not be blank.</param>
    /// <param name="amount">A positive amount.</param>
    /// <param name="date">The date the transaction occurred.</param>
    /// <param name="categoryId">The category the transaction is assigned to.</param>
    /// <returns>The created transaction.</returns>
    /// <exception cref="DomainValidationException">A supplied value violates a transaction invariant.</exception>
    public static Transaction Create(string description, decimal amount, DateOnly date, Guid categoryId) =>
        new(Guid.CreateVersion7(), NormalizeDescription(description), ValidateAmount(amount), date, ValidateCategoryId(categoryId));

    /// <summary>Overwrites all editable values of the transaction.</summary>
    /// <param name="description">Free-text description; must not be blank.</param>
    /// <param name="amount">A positive amount.</param>
    /// <param name="date">The date the transaction occurred.</param>
    /// <param name="categoryId">The category the transaction is assigned to.</param>
    /// <exception cref="DomainValidationException">A supplied value violates a transaction invariant.</exception>
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
