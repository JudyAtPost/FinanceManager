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

    public static Result<Category> Create(string name, TransactionType type) =>
        Validate(name, type).Map(validName => new Category(Guid.CreateVersion7(), validName, type));

    public Result Rename(string name) =>
        NormalizeName(name).Bind(validName =>
        {
            Name = validName;
            return Result.Success();
        });

    public Result ChangeType(TransactionType type) =>
        ValidateType(type).Bind(() =>
        {
            Type = type;
            return Result.Success();
        });

    private static Result<string> Validate(string name, TransactionType type) =>
        NormalizeName(name).Ensure(_ => Enum.IsDefined(type), Error.Validation($"Unknown transaction type '{type}'."));

    private static Result ValidateType(TransactionType type) =>
        Enum.IsDefined(type)
            ? Result.Success()
            : Error.Validation($"Unknown transaction type '{type}'.");

    private static Result<string> NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("Category name must not be empty.");
        }

        string trimmed = name.Trim();
        if (trimmed.Length > MaxNameLength)
        {
            return Error.Validation($"Category name must not exceed {MaxNameLength} characters.");
        }

        return trimmed;
    }
}
