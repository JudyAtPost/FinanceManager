namespace PersonalFinance.Domain;

/// <summary>
/// A label transactions are grouped under, for example "Groceries" or "Salary".
/// </summary>
public sealed class Category
{
    /// <summary>Maximum number of characters allowed in <see cref="Name"/>.</summary>
    public const int MaxNameLength = 60;

    private Category(Guid id, string name, TransactionType type)
    {
        Id = id;
        Name = name;
        Type = type;
    }

    /// <summary>Required by the persistence layer for materialization.</summary>
    private Category()
    {
        Name = string.Empty;
    }

    /// <summary>Gets the identifier of the category.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the display name of the category.</summary>
    public string Name { get; private set; }

    /// <summary>Gets the kind of transaction this category applies to.</summary>
    public TransactionType Type { get; private set; }

    /// <summary>Creates a new category.</summary>
    /// <param name="name">The display name; must not be blank.</param>
    /// <param name="type">Whether the category groups income or expenses.</param>
    /// <returns>The created category.</returns>
    /// <exception cref="DomainValidationException">The name is blank, too long, or the type is undefined.</exception>
    public static Category Create(string name, TransactionType type) =>
        new(Guid.CreateVersion7(), NormalizeName(name), ValidateType(type));

    /// <summary>Renames the category and changes the transaction type it applies to.</summary>
    /// <param name="name">The new display name; must not be blank.</param>
    /// <param name="type">The new transaction type.</param>
    /// <exception cref="DomainValidationException">The name is blank, too long, or the type is undefined.</exception>
    public void Update(string name, TransactionType type)
    {
        Name = NormalizeName(name);
        Type = ValidateType(type);
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("Category name must not be empty.");
        }

        string trimmed = name.Trim();
        if (trimmed.Length > MaxNameLength)
        {
            throw new DomainValidationException($"Category name must not exceed {MaxNameLength} characters.");
        }

        return trimmed;
    }

    private static TransactionType ValidateType(TransactionType type) =>
        Enum.IsDefined(type)
            ? type
            : throw new DomainValidationException($"Unknown transaction type '{type}'.");
}
