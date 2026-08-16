using System.Globalization;

namespace PersonalFinance.Domain;

public readonly record struct BudgetMonth : IComparable<BudgetMonth>
{
    public BudgetMonth(int year, int month)
    {
        if (year is < 1 or > 9999)
        {
            throw new DomainValidationException($"Year must be between 1 and 9999 but was {year}.");
        }

        if (month is < 1 or > 12)
        {
            throw new DomainValidationException($"Month must be between 1 and 12 but was {month}.");
        }

        Year = year;
        Month = month;
    }

    public int Year { get; }

    public int Month { get; }

    public DateOnly FirstDay => new(Year, Month, 1);

    public DateOnly LastDay => new(Year, Month, DateTime.DaysInMonth(Year, Month));

    public static BudgetMonth FromDate(DateOnly date) => new(date.Year, date.Month);

    public bool Contains(DateOnly date) => date.Year == Year && date.Month == Month;

    public int CompareTo(BudgetMonth other)
    {
        int yearComparison = Year.CompareTo(other.Year);
        return yearComparison != 0 ? yearComparison : Month.CompareTo(other.Month);
    }

    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Year:D4}-{Month:D2}");
}
