namespace PersonalFinance.Domain;

/// <summary>
/// Classifies a business-rule failure so callers can map it to a suitable response.
/// </summary>
public enum ErrorType
{
    /// <summary>An input or invariant violated a domain rule.</summary>
    Validation,

    /// <summary>A requested resource does not exist.</summary>
    NotFound,

    /// <summary>The request conflicts with the current state.</summary>
    Conflict
}

/// <summary>
/// Represents a business-rule failure as data instead of an exception.
/// </summary>
public sealed record Error(ErrorType Type, string Message)
{
    /// <summary>Creates a validation error.</summary>
    public static Error Validation(string message) => new(ErrorType.Validation, message);

    /// <summary>Creates a not-found error.</summary>
    public static Error NotFound(string message) => new(ErrorType.NotFound, message);

    /// <summary>Creates a conflict error.</summary>
    public static Error Conflict(string message) => new(ErrorType.Conflict, message);
}
