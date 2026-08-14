namespace PersonalFinance.Domain;

/// <summary>
/// Distinguishes money coming in from money going out.
/// </summary>
public enum TransactionType
{
    /// <summary>Money received, for example a salary payment.</summary>
    Income = 1,

    /// <summary>Money spent, for example groceries or rent.</summary>
    Expense = 2
}
