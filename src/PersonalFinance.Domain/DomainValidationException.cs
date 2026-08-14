namespace PersonalFinance.Domain;

/// <summary>
/// Thrown when an operation would leave a domain entity in an invalid state.
/// </summary>
public sealed class DomainValidationException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="DomainValidationException"/> class.</summary>
    /// <param name="message">A description of the violated rule.</param>
    public DomainValidationException(string message)
        : base(message)
    {
    }
}
