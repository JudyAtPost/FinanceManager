using PersonalFinance.Domain;

namespace PersonalFinance.Api.Extensions;

/// <summary>
/// Maps <see cref="Result"/> outcomes to HTTP responses without throwing exceptions.
/// </summary>
public static class ResultExtensions
{
    /// <summary>Converts a domain <see cref="Error"/> into a ProblemDetails HTTP response.</summary>
    /// <param name="error">The error to translate.</param>
    /// <returns>An <see cref="IResult"/> carrying the appropriate status code.</returns>
    public static IResult ToProblem(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        (int statusCode, string title) = error.Type switch
        {
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Resource not found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Request conflicts with the current state"),
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
        };

        return Results.Problem(statusCode: statusCode, title: title, detail: error.Message);
    }

    /// <summary>Projects a successful value into an HTTP response, or maps the error to a problem.</summary>
    /// <typeparam name="TValue">The type carried by the result.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">The projection to run when the result is successful.</param>
    /// <returns>The success response, or a ProblemDetails response on failure.</returns>
    public static IResult Match<TValue>(this Result<TValue> result, Func<TValue, IResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);

        return result.IsSuccess ? onSuccess(result.Value) : result.Error!.ToProblem();
    }

    /// <summary>Projects a successful value into an awaited HTTP response, or maps the error to a problem.</summary>
    /// <typeparam name="TValue">The type carried by the result.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">The asynchronous projection to run when the result is successful.</param>
    /// <returns>The success response, or a ProblemDetails response on failure.</returns>
    public static async Task<IResult> Match<TValue>(this Result<TValue> result, Func<TValue, Task<IResult>> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);

        return result.IsSuccess ? await onSuccess(result.Value) : result.Error!.ToProblem();
    }

    /// <summary>Produces an HTTP response for a valueless result, or maps the error to a problem.</summary>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">The response factory to run when the result is successful.</param>
    /// <returns>The success response, or a ProblemDetails response on failure.</returns>
    public static IResult Match(this Result result, Func<IResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);

        return result.IsSuccess ? onSuccess() : result.Error!.ToProblem();
    }
}
