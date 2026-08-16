namespace PersonalFinance.Domain.Extensions;

/// <summary>
/// Async combinators that let repository-backed workflows compose into a single
/// <see cref="Result"/> pipeline without manual <c>IsFailure</c> or null checks.
/// </summary>
public static class ResultAsyncExtensions
{
    /// <summary>Wraps a nullable lookup as a <see cref="Result{TValue}"/>, failing with <paramref name="notFound"/> when absent.</summary>
    public static Result<TValue> Require<TValue>(this TValue? value, Error notFound)
        where TValue : class =>
        value is not null ? value : notFound;

    /// <summary>Awaits a lookup and wraps it as a <see cref="Result{TValue}"/>, failing when absent.</summary>
    public static async Task<Result<TValue>> Require<TValue>(this Task<TValue?> lookup, Error notFound)
        where TValue : class =>
        (await lookup.ConfigureAwait(false)).Require(notFound);

    /// <summary>Continues an async chain with <paramref name="next"/>, or propagates the failure.</summary>
    public static async Task<Result<TResult>> Bind<TValue, TResult>(
        this Task<Result<TValue>> result,
        Func<TValue, Task<Result<TResult>>> next)
    {
        Result<TValue> awaited = await result.ConfigureAwait(false);
        return awaited.IsFailure ? awaited.Error! : await next(awaited.Value).ConfigureAwait(false);
    }

    /// <summary>Continues an async chain with a synchronous <paramref name="next"/>, or propagates the failure.</summary>
    public static async Task<Result<TResult>> Bind<TValue, TResult>(
        this Task<Result<TValue>> result,
        Func<TValue, Result<TResult>> next)
    {
        Result<TValue> awaited = await result.ConfigureAwait(false);
        return awaited.IsFailure ? awaited.Error! : next(awaited.Value);
    }

    /// <summary>Transforms a successful async value, or propagates the failure.</summary>
    public static async Task<Result<TResult>> Map<TValue, TResult>(
        this Task<Result<TValue>> result,
        Func<TValue, TResult> map)
    {
        Result<TValue> awaited = await result.ConfigureAwait(false);
        return awaited.IsFailure ? awaited.Error! : map(awaited.Value);
    }

    /// <summary>Runs an async side effect on success, then propagates the original result.</summary>
    public static async Task<Result<TValue>> Tap<TValue>(
        this Task<Result<TValue>> result,
        Func<TValue, Task> effect)
    {
        Result<TValue> awaited = await result.ConfigureAwait(false);
        if (awaited.IsSuccess)
        {
            await effect(awaited.Value).ConfigureAwait(false);
        }

        return awaited;
    }

    /// <summary>Runs a side effect on success, then propagates the original result.</summary>
    public static async Task<Result<TValue>> Tap<TValue>(
        this Task<Result<TValue>> result,
        Action<TValue> effect)
    {
        Result<TValue> awaited = await result.ConfigureAwait(false);
        if (awaited.IsSuccess)
        {
            effect(awaited.Value);
        }

        return awaited;
    }

    /// <summary>Runs a side effect on success, then propagates the original result.</summary>
    public static Result<TValue> Tap<TValue>(this Result<TValue> result, Action<TValue> effect)
    {
        if (result.IsSuccess)
        {
            effect(result.Value);
        }

        return result;
    }

    /// <summary>Collapses a value-carrying result into a valueless <see cref="Result"/>.</summary>
    public static async Task<Result> ToResult<TValue>(this Task<Result<TValue>> result)
    {
        Result<TValue> awaited = await result.ConfigureAwait(false);
        return awaited.IsSuccess ? Result.Success() : awaited.Error!;
    }
}
