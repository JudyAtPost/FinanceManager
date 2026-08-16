namespace PersonalFinance.Domain;

public sealed class Category
{
    public const int MaxNameLength = 60;

    private Category(Guid id, string name, TransactionType type)
    {
        Id = id;
        Name = name;
        Type = type;
    }

    private Category()
    {
        Name = string.Empty;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public TransactionType Type { get; private set; }

    public static Category Create(string name, TransactionType type) =>
        new(Guid.CreateVersion7(), NormalizeName(name), ValidateType(type));

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
