using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Common;
using PersonalFinance.Domain;

namespace PersonalFinance.Application.Categories;

/// <summary>
/// Creates, updates, deletes, and lists categories.
/// </summary>
public sealed class CategoryService
{
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Initializes a new instance of the <see cref="CategoryService"/> class.</summary>
    /// <param name="categories">Category storage.</param>
    /// <param name="unitOfWork">Used to commit changes.</param>
    public CategoryService(ICategoryRepository categories, IUnitOfWork unitOfWork)
    {
        _categories = categories;
        _unitOfWork = unitOfWork;
    }

    /// <summary>Lists all categories.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>All known categories.</returns>
    public async Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Category> categories = await _categories.ListAsync(cancellationToken).ConfigureAwait(false);
        return [.. categories.Select(CategoryDto.FromDomain)];
    }

    /// <summary>Loads a single category.</summary>
    /// <param name="id">The identifier of the category.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The category.</returns>
    /// <exception cref="NotFoundException">The category does not exist.</exception>
    public async Task<CategoryDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        Category category = await _categories.GetAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Category '{id}' was not found.");

        return CategoryDto.FromDomain(category);
    }

    /// <summary>Creates a new category.</summary>
    /// <param name="request">The category to create.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The created category.</returns>
    public async Task<CategoryDto> CreateAsync(SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Category category = Category.Create(request.Name, request.Type);
        _categories.Add(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return CategoryDto.FromDomain(category);
    }

    /// <summary>Updates an existing category.</summary>
    /// <param name="id">The identifier of the category.</param>
    /// <param name="request">The new values.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The updated category.</returns>
    /// <exception cref="NotFoundException">The category does not exist.</exception>
    public async Task<CategoryDto> UpdateAsync(Guid id, SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Category category = await _categories.GetAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Category '{id}' was not found.");

        category.Update(request.Name, request.Type);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return CategoryDto.FromDomain(category);
    }

    /// <summary>Deletes a category that is no longer referenced.</summary>
    /// <param name="id">The identifier of the category.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes once the category is deleted.</returns>
    /// <exception cref="NotFoundException">The category does not exist.</exception>
    /// <exception cref="ConflictException">The category is still referenced by transactions or budgets.</exception>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Category category = await _categories.GetAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Category '{id}' was not found.");

        if (await _categories.IsInUseAsync(id, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException($"Category '{category.Name}' is still used by transactions or budgets.");
        }

        _categories.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
