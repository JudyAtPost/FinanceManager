using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Extensions;
using PersonalFinance.Domain;
using PersonalFinance.Domain.Extensions;

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

    public async Task<Result<CategoryDto>> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await FindAsync(id, cancellationToken).Map(CategoryDto.FromDomain).ConfigureAwait(false);

    public async Task<Result<CategoryDto>> CreateAsync(SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await Category.Create(request.Name, request.Type)
            .Tap(_categories.Add)
            .SaveAsync(_unitOfWork, cancellationToken)
            .Map(CategoryDto.FromDomain)
            .ConfigureAwait(false);
    }

    public async Task<Result<CategoryDto>> UpdateAsync(Guid id, SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await FindAsync(id, cancellationToken)
            .Bind(category => category.Rename(request.Name).Map(() => category))
            .Bind(category => ApplyTypeChangeAsync(category, request.Type, cancellationToken))
            .SaveAsync(_unitOfWork, cancellationToken)
            .Map(CategoryDto.FromDomain)
            .ConfigureAwait(false);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken) =>
        await FindAsync(id, cancellationToken)
            .Bind(async category => (await _categories.IsInUseAsync(id, cancellationToken).ConfigureAwait(false))
                ? Error.Conflict($"Category '{category.Name}' is still used by transactions or budgets.")
                : Result.Success<Category>(category))
            .Tap(_categories.Remove)
            .SaveAsync(_unitOfWork, cancellationToken)
            .ToResult()
            .ConfigureAwait(false);

    private async Task<Result<Category>> ApplyTypeChangeAsync(
        Category category,
        TransactionType requestedType,
        CancellationToken cancellationToken)
    {
        if (category.Type == requestedType)
        {
            return category;
        }

        if (await _categories.IsInUseAsync(category.Id, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                $"Category '{category.Name}' is already used by transactions or budgets and can no longer change its type.");
        }

        return category.ChangeType(requestedType).Map(() => category);
    }

    private Task<Result<Category>> FindAsync(Guid id, CancellationToken cancellationToken) =>
        _categories.GetAsync(id, cancellationToken).Require(Error.NotFound($"Category '{id}' was not found."));
}
