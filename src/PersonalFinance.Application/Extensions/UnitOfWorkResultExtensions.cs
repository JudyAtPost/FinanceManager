using PersonalFinance.Application.Abstractions;
using PersonalFinance.Domain;
using PersonalFinance.Domain.Extensions;

namespace PersonalFinance.Application.Extensions;

public static class UnitOfWorkResultExtensions
{
    public static Task<Result<TValue>> SaveAsync<TValue>(
        this Result<TValue> result,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken) =>
        Task.FromResult(result).SaveAsync(unitOfWork, cancellationToken);

    public static async Task<Result<TValue>> SaveAsync<TValue>(
        this Task<Result<TValue>> result,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);

        Result<TValue> awaited = await result.ConfigureAwait(false);
        if (awaited.IsFailure)
        {
            return awaited;
        }

        Result saved = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return saved.IsSuccess ? awaited : saved.Error!;
    }
}
