using System.Globalization;

namespace PersonalFinance.Domain;

public readonly record struct BudgetMonth : IComparable<BudgetMonth>
{
    private BudgetMonth(int year, int month)
    {
        Year = year;
        Month = month;
    }

    public int Year { get; }

    public int Month { get; }

    public DateOnly FirstDay => new(Year, Month, 1);

    public DateOnly LastDay => new(Year, Month, DateTime.DaysInMonth(Year, Month));

    public static Result<BudgetMonth> Create(int year, int month)
    {
        if (year is < 1 or > 9999)
        {
            return Error.Validation($"Year must be between 1 and 9999 but was {year}.");
        }

        if (month is < 1 or > 12)
        {
            return Error.Validation($"Month must be between 1 and 12 but was {month}.");
        }

        return new BudgetMonth(year, month);
    }

    public static Result<BudgetMonth?> CreateOptional(int? year, int? month)
    {
        if (year is null || month is null)
        {
            return Result.Success<BudgetMonth?>(null);
        }

        return Create(year.Value, month.Value).Map(value => (BudgetMonth?)value);
    }

    public static BudgetMonth FromDate(DateOnly date) => new(date.Year, date.Month);

    public bool Contains(DateOnly date) => date.Year == Year && date.Month == Month;

    public int CompareTo(BudgetMonth other)
    {
        int yearComparison = Year.CompareTo(other.Year);
        return yearComparison != 0 ? yearComparison : Month.CompareTo(other.Month);
    }

    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Year:D4}-{Month:D2}");
}
