using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Common;
using PersonalFinance.Domain;

namespace PersonalFinance.Application.Transactions;

/// <summary>
/// Records and manages income and expenses.
/// </summary>
public sealed class TransactionService
{
    private readonly ITransactionRepository _transactions;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Initializes a new instance of the <see cref="TransactionService"/> class.</summary>
    /// <param name="transactions">Transaction storage.</param>
    /// <param name="categories">Category storage, used to validate assignments.</param>
    /// <param name="unitOfWork">Used to commit changes.</param>
    public TransactionService(
        ITransactionRepository transactions,
        ICategoryRepository categories,
        IUnitOfWork unitOfWork)
    {
        _transactions = transactions;
        _categories = categories;
        _unitOfWork = unitOfWork;
    }

    /// <summary>Lists transactions matching the supplied filter.</summary>
    /// <param name="query">The filter and paging options.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A page of matching transactions.</returns>
    public async Task<PagedResult<TransactionDto>> ListAsync(TransactionQuery query, CancellationToken cancellationToken)
    {
        PagedResult<Transaction> page = await _transactions.ListAsync(query, cancellationToken).ConfigureAwait(false);

        return new PagedResult<TransactionDto>(
            [.. page.Items.Select(TransactionDto.FromDomain)],
            page.TotalCount,
            page.Page,
            page.PageSize);
    }

    /// <summary>Loads a single transaction.</summary>
    /// <param name="id">The identifier of the transaction.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The transaction.</returns>
    /// <exception cref="NotFoundException">The transaction does not exist.</exception>
    public async Task<TransactionDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        Transaction transaction = await _transactions.GetAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Transaction '{id}' was not found.");

        return TransactionDto.FromDomain(transaction);
    }

    /// <summary>Records a new transaction.</summary>
    /// <param name="request">The transaction to record.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The recorded transaction.</returns>
    /// <exception cref="NotFoundException">The referenced category does not exist.</exception>
    public async Task<TransactionDto> CreateAsync(SaveTransactionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Category category = await _categories.GetAsync(request.CategoryId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Category '{request.CategoryId}' was not found.");

        Transaction transaction = Transaction.Create(request.Description, request.Amount, request.Date, request.CategoryId);
        _transactions.Add(transaction);
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

    /// <summary>Updates an existing transaction.</summary>
    /// <param name="id">The identifier of the transaction.</param>
    /// <param name="request">The new values.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The updated transaction.</returns>
    /// <exception cref="NotFoundException">The transaction or the referenced category does not exist.</exception>
    public async Task<TransactionDto> UpdateAsync(Guid id, SaveTransactionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Transaction transaction = await _transactions.GetAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Transaction '{id}' was not found.");

        Category category = await _categories.GetAsync(request.CategoryId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Category '{request.CategoryId}' was not found.");

        transaction.Update(request.Description, request.Amount, request.Date, request.CategoryId);
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

    /// <summary>Deletes a transaction.</summary>
    /// <param name="id">The identifier of the transaction.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes once the transaction is deleted.</returns>
    /// <exception cref="NotFoundException">The transaction does not exist.</exception>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Transaction transaction = await _transactions.GetAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Transaction '{id}' was not found.");

        _transactions.Remove(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
