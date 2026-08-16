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

    public async Task<TransactionDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        Transaction transaction = await _transactions.GetAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Transaction '{id}' was not found.");

        return TransactionDto.FromDomain(transaction);
    }

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

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Transaction transaction = await _transactions.GetAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Transaction '{id}' was not found.");

        _transactions.Remove(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
