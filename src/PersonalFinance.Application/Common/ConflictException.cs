namespace PersonalFinance.Application.Common;

/// <summary>
/// Thrown when a request conflicts with the current state, for example a duplicate budget.
/// </summary>
public sealed class ConflictException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="ConflictException"/> class.</summary>
    /// <param name="message">A description of the conflict.</param>
    public ConflictException(string message)
        : base(message)
    {
    }
}
