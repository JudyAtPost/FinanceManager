namespace PersonalFinance.Domain;

/// <summary>
/// Represents the outcome of an operation that can fail because of a business rule.
/// </summary>
public class Result
{
    private protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
        {
            throw new InvalidOperationException("A successful result cannot carry an error.");
        }

        if (!isSuccess && error is null)
        {
            throw new InvalidOperationException("A failed result must carry an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the error describing the failure, or <c>null</c> when successful.</summary>
    public Error? Error { get; }

    /// <summary>Creates a successful result.</summary>
    public static Result Success() => new(true, null);

    /// <summary>Creates a failed result.</summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>Creates a successful result carrying a value.</summary>
    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);

    /// <summary>Creates a failed result for a value-producing operation.</summary>
    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);

    /// <summary>Allows returning an <see cref="Error"/> directly as a failed result.</summary>
    public static implicit operator Result(Error error) => Failure(error);

    /// <summary>Fails the result when <paramref name="predicate"/> is not met.</summary>
    public Result Ensure(Func<bool> predicate, Error error) =>
        IsFailure || predicate() ? this : error;

    /// <summary>Continues the chain with <paramref name="next"/> only when still successful.</summary>
    public Result Bind(Func<Result> next) => IsFailure ? this : next();

    /// <summary>Produces a value from a successful result, or propagates the failure.</summary>
    public Result<TValue> Bind<TValue>(Func<Result<TValue>> next) =>
        IsFailure ? Error! : next();

    /// <summary>Projects a successful valueless result onto <paramref name="value"/>.</summary>
    public Result<TValue> Map<TValue>(Func<TValue> value) =>
        IsFailure ? Error! : value();
}

/// <summary>
/// Represents the outcome of an operation that yields a value when it succeeds.
/// </summary>
/// <typeparam name="TValue">The type of the produced value.</typeparam>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(bool isSuccess, TValue? value, Error? error)
        : base(isSuccess, error) => _value = value;

    /// <summary>Gets the produced value, or throws when the result is a failure.</summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    /// <summary>Creates a successful result carrying a value.</summary>
    public static Result<TValue> Success(TValue value) => new(true, value, null);

    /// <summary>Creates a failed result.</summary>
    public static new Result<TValue> Failure(Error error) => new(false, default, error);

    /// <summary>Allows returning a value directly as a successful result.</summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>Allows returning an <see cref="Error"/> directly as a failed result.</summary>
    public static implicit operator Result<TValue>(Error error) => Failure(error);

    /// <summary>Fails the result when the carried value does not satisfy <paramref name="predicate"/>.</summary>
    public Result<TValue> Ensure(Func<TValue, bool> predicate, Error error) =>
        IsFailure || predicate(Value) ? this : error;

    /// <summary>Transforms a successful value, or propagates the failure.</summary>
    public Result<TResult> Map<TResult>(Func<TValue, TResult> map) =>
        IsFailure ? Error! : map(Value);

    /// <summary>Continues the chain with <paramref name="next"/>, or propagates the failure.</summary>
    public Result<TResult> Bind<TResult>(Func<TValue, Result<TResult>> next) =>
        IsFailure ? Error! : next(Value);

    /// <summary>Continues with a valueless <paramref name="next"/>, or propagates the failure.</summary>
    public Result Bind(Func<TValue, Result> next) =>
        IsFailure ? Error! : next(Value);
}
