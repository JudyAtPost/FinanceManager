using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Common;
using PersonalFinance.Application.Extensions;
using PersonalFinance.Domain;
using PersonalFinance.Domain.Extensions;

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

    public async Task<Result<BudgetDto>> CreateAsync(CreateBudgetRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await FindCategoryAsync(request.CategoryId, cancellationToken)
            .Bind(category => category.Type == TransactionType.Expense
                ? Result.Success(category)
                : Error.Conflict("Budgets can only be defined for expense categories."))
            .Bind(category => BudgetMonth.Create(request.Year, request.Month).Map(month => (category, month)))
            .Bind(pair => EnsureUniqueAsync(request.CategoryId, pair.category, pair.month, cancellationToken))
            .Bind(pair => Budget
                .Create(request.CategoryId, pair.month, request.Limit)
                .Map(budget => (budget, pair.category, pair.month)))
            .Tap(result => _budgets.Add(result.budget))
            .SaveAsync(_unitOfWork, cancellationToken)
            .Map(result => new BudgetDto(
                result.budget.Id, result.category.Id, result.category.Name, result.month.Year, result.month.Month, result.budget.Limit))
            .ConfigureAwait(false);
    }

    public async Task<Result<BudgetDto>> UpdateAsync(Guid id, UpdateBudgetRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await FindBudgetAsync(id, cancellationToken)
            .Bind(budget => budget.ChangeLimit(request.Limit).Map(() => budget))
            .SaveAsync(_unitOfWork, cancellationToken)
            .Map(BudgetDto.FromDomain)
            .ConfigureAwait(false);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken) =>
        await FindBudgetAsync(id, cancellationToken)
            .Tap(_budgets.Remove)
            .SaveAsync(_unitOfWork, cancellationToken)
            .ToResult()
            .ConfigureAwait(false);

    private Task<Result<Budget>> FindBudgetAsync(Guid id, CancellationToken cancellationToken) =>
        _budgets.GetAsync(id, cancellationToken).Require(Error.NotFound($"Budget '{id}' was not found."));

    private Task<Result<Category>> FindCategoryAsync(Guid id, CancellationToken cancellationToken) =>
        _categories.GetAsync(id, cancellationToken).Require(Error.NotFound($"Category '{id}' was not found."));

    private async Task<Result<(Category category, BudgetMonth month)>> EnsureUniqueAsync(
        Guid categoryId,
        Category category,
        BudgetMonth month,
        CancellationToken cancellationToken)
    {
        Budget? existing = await _budgets.GetForCategoryAndMonthAsync(categoryId, month, cancellationToken).ConfigureAwait(false);
        return existing is null
            ? (category, month)
            : Error.Conflict($"A budget for category '{category.Name}' already exists for {month}.");
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
