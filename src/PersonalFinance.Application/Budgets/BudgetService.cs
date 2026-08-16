using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Common;
using PersonalFinance.Domain;

namespace PersonalFinance.Application.Budgets;

public sealed class BudgetService
{
    private readonly IBudgetRepository _budgets;
    private readonly ICategoryRepository _categories;
    private readonly ITransactionRepository _transactions;
    private readonly IUnitOfWork _unitOfWork;

    public BudgetService(
        IBudgetRepository budgets,
        ICategoryRepository categories,
        ITransactionRepository transactions,
        IUnitOfWork unitOfWork)
    {
        _budgets = budgets;
        _categories = categories;
        _transactions = transactions;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<BudgetDto>> ListAsync(BudgetMonth month, CancellationToken cancellationToken)
    {
        IReadOnlyList<Budget> budgets = await _budgets.ListForMonthAsync(month, cancellationToken).ConfigureAwait(false);
        return [.. budgets.Select(BudgetDto.FromDomain)];
    }

    public async Task<BudgetDto> CreateAsync(CreateBudgetRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Category category = await _categories.GetAsync(request.CategoryId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Category '{request.CategoryId}' was not found.");

        if (category.Type != TransactionType.Expense)
        {
            throw new ConflictException("Budgets can only be defined for expense categories.");
        }

        var month = new BudgetMonth(request.Year, request.Month);

        Budget? existing = await _budgets.GetForCategoryAndMonthAsync(request.CategoryId, month, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            throw new ConflictException($"A budget for category '{category.Name}' already exists for {month}.");
        }

        Budget budget = Budget.Create(request.CategoryId, month, request.Limit);
        _budgets.Add(budget);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new BudgetDto(budget.Id, category.Id, category.Name, month.Year, month.Month, budget.Limit);
    }

    public async Task<BudgetDto> UpdateAsync(Guid id, UpdateBudgetRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Budget budget = await _budgets.GetAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Budget '{id}' was not found.");

        budget.ChangeLimit(request.Limit);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return BudgetDto.FromDomain(budget);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Budget budget = await _budgets.GetAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Budget '{id}' was not found.");

        _budgets.Remove(budget);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<BudgetComparison>> CompareAsync(BudgetMonth month, CancellationToken cancellationToken)
    {
        IReadOnlyList<Budget> budgets = await _budgets.ListForMonthAsync(month, cancellationToken).ConfigureAwait(false);
        if (budgets.Count == 0)
        {
            return [];
        }

        IReadOnlyList<CategoryTotal> totals = await _transactions
            .GetMonthlyTotalsByCategoryAsync(month, cancellationToken)
            .ConfigureAwait(false);

        return Compare(budgets, totals);
    }

    public static IReadOnlyList<BudgetComparison> Compare(
        IReadOnlyList<Budget> budgets,
        IReadOnlyList<CategoryTotal> totals)
    {
        ArgumentNullException.ThrowIfNull(budgets);
        ArgumentNullException.ThrowIfNull(totals);

        Dictionary<Guid, decimal> spentByCategory = totals
            .Where(total => total.Type == TransactionType.Expense)
            .GroupBy(total => total.CategoryId)
            .ToDictionary(group => group.Key, group => group.Sum(total => total.Total));

        return
        [
            .. budgets
                .Select(budget => new BudgetComparison(
                    budget.CategoryId,
                    budget.Category?.Name
                        ?? totals.FirstOrDefault(total => total.CategoryId == budget.CategoryId)?.CategoryName
                        ?? string.Empty,
                    budget.Limit,
                    spentByCategory.GetValueOrDefault(budget.CategoryId)))
                .OrderByDescending(comparison => comparison.OverspentBy)
                .ThenByDescending(comparison => comparison.Spent)
                .ThenBy(comparison => comparison.CategoryName, StringComparer.OrdinalIgnoreCase)
        ];
    }
}
