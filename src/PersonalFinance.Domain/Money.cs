namespace PersonalFinance.Domain;

public static class Money
{
    /// <summary>The number of decimal places used for every persisted monetary amount.</summary>
    public const int Scale = 2;

    /// <summary>Rounds an amount to the persisted monetary scale.</summary>
    public static decimal Round(decimal amount) => decimal.Round(amount, Scale, MidpointRounding.AwayFromZero);

    public static decimal Percentage(decimal value, decimal total) =>
        total <= 0m ? 0m : Round(value / total * 100m);
}
