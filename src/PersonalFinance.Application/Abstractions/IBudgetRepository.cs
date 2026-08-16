using PersonalFinance.Domain;

namespace PersonalFinance.Application.Abstractions;

public interface IBudgetRepository
{
    Task<IReadOnlyList<Budget>> ListForMonthAsync(BudgetMonth month, CancellationToken cancellationToken);

    Task<Budget?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<Budget?> GetForCategoryAndMonthAsync(Guid categoryId, BudgetMonth month, CancellationToken cancellationToken);

    void Add(Budget budget);

    void Remove(Budget budget);
}
