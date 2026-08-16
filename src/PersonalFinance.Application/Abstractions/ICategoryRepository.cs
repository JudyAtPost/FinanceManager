using PersonalFinance.Domain;

namespace PersonalFinance.Application.Abstractions;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> ListAsync(CancellationToken cancellationToken);

    Task<Category?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> IsInUseAsync(Guid id, CancellationToken cancellationToken);

    void Add(Category category);

    void Remove(Category category);
}
