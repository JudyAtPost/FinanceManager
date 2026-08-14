namespace PersonalFinance.Application.Common;

/// <summary>
/// Thrown when a requested entity does not exist.
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="NotFoundException"/> class.</summary>
    /// <param name="message">A description of what could not be found.</param>
    public NotFoundException(string message)
        : base(message)
    {
    }
}
