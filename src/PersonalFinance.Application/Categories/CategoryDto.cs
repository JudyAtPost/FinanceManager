using PersonalFinance.Domain;

namespace PersonalFinance.Application.Categories;

public sealed record CategoryDto(Guid Id, string Name, TransactionType Type)
{
    public static CategoryDto FromDomain(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        return new CategoryDto(category.Id, category.Name, category.Type);
    }
}

public sealed record SaveCategoryRequest(string Name, TransactionType Type);
