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

    public async Task<Result<TransactionDto>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        Transaction? transaction = await _transactions.GetAsync(id, cancellationToken).ConfigureAwait(false);
        if (transaction is null)
        {
            return Error.NotFound($"Transaction '{id}' was not found.");
        }

        return TransactionDto.FromDomain(transaction);
    }

    public async Task<Result<TransactionDto>> CreateAsync(SaveTransactionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Category? category = await _categories.GetAsync(request.CategoryId, cancellationToken).ConfigureAwait(false);
        if (category is null)
        {
            return Error.NotFound($"Category '{request.CategoryId}' was not found.");
        }

        Result<Transaction> transaction = Transaction.Create(request.Description, request.Amount, request.Date, request.CategoryId);
        if (transaction.IsFailure)
        {
            return transaction.Error!;
        }

        _transactions.Add(transaction.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new TransactionDto(
            transaction.Value.Id,
            transaction.Value.Description,
            transaction.Value.Amount,
            transaction.Value.Date,
            category.Id,
            category.Name,
            category.Type);
    }

    public async Task<Result<TransactionDto>> UpdateAsync(Guid id, SaveTransactionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Transaction? transaction = await _transactions.GetAsync(id, cancellationToken).ConfigureAwait(false);
        if (transaction is null)
        {
            return Error.NotFound($"Transaction '{id}' was not found.");
        }

        Category? category = await _categories.GetAsync(request.CategoryId, cancellationToken).ConfigureAwait(false);
        if (category is null)
        {
            return Error.NotFound($"Category '{request.CategoryId}' was not found.");
        }

        Result update = transaction.Update(request.Description, request.Amount, request.Date, request.CategoryId);
        if (update.IsFailure)
        {
            return update.Error!;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new TransactionDto(
            transaction.Id,
            transaction.Description,
            transaction.Amount,
            transaction.Date,
            category.Id,
            category.Name,
            category.Type);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Transaction? transaction = await _transactions.GetAsync(id, cancellationToken).ConfigureAwait(false);
        if (transaction is null)
        {
            return Error.NotFound($"Transaction '{id}' was not found.");
        }

        _transactions.Remove(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
