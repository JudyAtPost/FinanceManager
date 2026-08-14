using PersonalFinance.Domain;

namespace PersonalFinance.Application.Abstractions;

/// <summary>
/// Read and write access to <see cref="Category"/> aggregates.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>Lists all categories ordered by name.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>All known categories.</returns>
    Task<IReadOnlyList<Category>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Loads a single category.</summary>
    /// <param name="id">The identifier of the category.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The category, or <see langword="null"/> when it does not exist.</returns>
    Task<Category?> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Determines whether a category with the supplied identifier exists.</summary>
    /// <param name="id">The identifier of the category.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when the category exists.</returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Determines whether the category is referenced by transactions or budgets.</summary>
    /// <param name="id">The identifier of the category.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns><see langword="true"/> when the category is still in use.</returns>
    Task<bool> IsInUseAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Adds a new category.</summary>
    /// <param name="category">The category to add.</param>
    void Add(Category category);

    /// <summary>Removes an existing category.</summary>
    /// <param name="category">The category to remove.</param>
    void Remove(Category category);
}
