using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Common;
using PersonalFinance.Domain;

namespace PersonalFinance.Application.Transactions;

public sealed class TransactionService
{
    private readonly ITransactionRepository _transactions;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;

    public TransactionService(
        ITransactionRepository transactions,
        ICategoryRepository categories,
        IUnitOfWork unitOfWork)
    {
        _transactions = transactions;
        _categories = categories;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<TransactionDto>> ListAsync(TransactionQuery query, CancellationToken cancellationToken)
    {
        PagedResult<Transaction> page = await _transactions.ListAsync(query, cancellationToken).ConfigureAwait(false);

        return new PagedResult<TransactionDto>(
            [.. page.Items.Select(TransactionDto.FromDomain)],
            page.TotalCount,
            page.Page,
            page.PageSize);
    }

    public async Task<Result<TransactionDto>> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await FindTransactionAsync(id, cancellationToken).Map(TransactionDto.FromDomain).ConfigureAwait(false);

    public async Task<Result<TransactionDto>> CreateAsync(SaveTransactionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await FindCategoryAsync(request.CategoryId, cancellationToken)
            .Bind(category => Transaction
                .Create(request.Description, request.Amount, request.Date, request.CategoryId)
                .Map(transaction => (transaction, category)))
            .Tap(pair => _transactions.Add(pair.transaction))
            .SaveAsync(_unitOfWork, cancellationToken)
            .Map(pair => ToDto(pair.transaction, pair.category))
            .ConfigureAwait(false);
    }

    public async Task<Result<TransactionDto>> UpdateAsync(Guid id, SaveTransactionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await FindTransactionAsync(id, cancellationToken)
            .Bind(transaction => FindCategoryAsync(request.CategoryId, cancellationToken)
                .Map(category => (transaction, category)))
            .Bind(pair => pair.transaction
                .Update(request.Description, request.Amount, request.Date, request.CategoryId)
                .Map(() => pair))
            .SaveAsync(_unitOfWork, cancellationToken)
            .Map(pair => ToDto(pair.transaction, pair.category))
            .ConfigureAwait(false);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken) =>
        await FindTransactionAsync(id, cancellationToken)
            .Tap(_transactions.Remove)
            .SaveAsync(_unitOfWork, cancellationToken)
            .ToResult()
            .ConfigureAwait(false);

    private Task<Result<Transaction>> FindTransactionAsync(Guid id, CancellationToken cancellationToken) =>
        _transactions.GetAsync(id, cancellationToken).Require(Error.NotFound($"Transaction '{id}' was not found."));

    private Task<Result<Category>> FindCategoryAsync(Guid id, CancellationToken cancellationToken) =>
        _categories.GetAsync(id, cancellationToken).Require(Error.NotFound($"Category '{id}' was not found."));

    private static TransactionDto ToDto(Transaction transaction, Category category) => new(
        transaction.Id,
        transaction.Description,
        transaction.Amount,
        transaction.Date,
        category.Id,
        category.Name,
        category.Type);
}
