using PersonalFinance.Application.Abstractions;
using PersonalFinance.Domain;

namespace PersonalFinance.Application.Common;

/// <summary>
/// Bridges <see cref="Result"/> pipelines with the unit of work so services can
/// persist changes as a single chained step.
/// </summary>
public static class UnitOfWorkResultExtensions
{
    /// <summary>Persists pending changes when the result is successful, then propagates it.</summary>
    public static Task<Result<TValue>> SaveAsync<TValue>(
        this Result<TValue> result,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken) =>
        Task.FromResult(result).SaveAsync(unitOfWork, cancellationToken);

    /// <summary>Persists pending changes when the awaited result is successful, then propagates it.</summary>
    public static Task<Result<TValue>> SaveAsync<TValue>(
        this Task<Result<TValue>> result,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken) =>
        result.Tap(_ => unitOfWork.SaveChangesAsync(cancellationToken));
}
