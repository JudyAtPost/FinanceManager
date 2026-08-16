using PersonalFinance.Application.Abstractions;
using PersonalFinance.Domain;

namespace PersonalFinance.Application.Categories;

public sealed class CategoryService
{
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(ICategoryRepository categories, IUnitOfWork unitOfWork)
    {
        _categories = categories;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Category> categories = await _categories.ListAsync(cancellationToken).ConfigureAwait(false);
        return [.. categories.Select(CategoryDto.FromDomain)];
    }

    public async Task<Result<CategoryDto>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        Category? category = await _categories.GetAsync(id, cancellationToken).ConfigureAwait(false);
        if (category is null)
        {
            return Error.NotFound($"Category '{id}' was not found.");
        }

        return CategoryDto.FromDomain(category);
    }

    public async Task<Result<CategoryDto>> CreateAsync(SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<Category> category = Category.Create(request.Name, request.Type);
        if (category.IsFailure)
        {
            return category.Error!;
        }

        _categories.Add(category.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return CategoryDto.FromDomain(category.Value);
    }

    public async Task<Result<CategoryDto>> UpdateAsync(Guid id, SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Category? category = await _categories.GetAsync(id, cancellationToken).ConfigureAwait(false);
        if (category is null)
        {
            return Error.NotFound($"Category '{id}' was not found.");
        }

        Result update = category.Update(request.Name, request.Type);
        if (update.IsFailure)
        {
            return update.Error!;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return CategoryDto.FromDomain(category);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Category? category = await _categories.GetAsync(id, cancellationToken).ConfigureAwait(false);
        if (category is null)
        {
            return Error.NotFound($"Category '{id}' was not found.");
        }

        if (await _categories.IsInUseAsync(id, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict($"Category '{category.Name}' is still used by transactions or budgets.");
        }

        _categories.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
