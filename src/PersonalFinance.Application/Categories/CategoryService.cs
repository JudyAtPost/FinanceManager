using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Common;
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

    public async Task<CategoryDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        Category category = await _categories.GetAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Category '{id}' was not found.");

        return CategoryDto.FromDomain(category);
    }

    public async Task<CategoryDto> CreateAsync(SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Category category = Category.Create(request.Name, request.Type);
        _categories.Add(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return CategoryDto.FromDomain(category);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Category category = await _categories.GetAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Category '{id}' was not found.");

        category.Update(request.Name, request.Type);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return CategoryDto.FromDomain(category);
    }

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
