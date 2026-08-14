using PersonalFinance.Domain;

namespace PersonalFinance.Application.Categories;

/// <summary>
/// A category as returned by the API.
/// </summary>
/// <param name="Id">The identifier of the category.</param>
/// <param name="Name">The display name.</param>
/// <param name="Type">Whether the category groups income or expenses.</param>
public sealed record CategoryDto(Guid Id, string Name, TransactionType Type)
{
    /// <summary>Projects a domain category onto its transport representation.</summary>
    /// <param name="category">The category to project.</param>
    /// <returns>The transport representation.</returns>
    public static CategoryDto FromDomain(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        return new CategoryDto(category.Id, category.Name, category.Type);
    }
}

/// <summary>
/// Payload used to create or update a category.
/// </summary>
/// <param name="Name">The display name; must not be blank.</param>
/// <param name="Type">Whether the category groups income or expenses.</param>
public sealed record SaveCategoryRequest(string Name, TransactionType Type);
