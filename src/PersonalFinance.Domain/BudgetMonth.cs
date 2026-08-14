using System.Globalization;

namespace PersonalFinance.Domain;

/// <summary>
/// A calendar month (year plus month number), the unit budgets and summaries are expressed in.
/// </summary>
public readonly record struct BudgetMonth : IComparable<BudgetMonth>
{
    /// <summary>Initializes a new instance of the <see cref="BudgetMonth"/> struct.</summary>
    /// <param name="year">Calendar year between 1 and 9999.</param>
    /// <param name="month">Month number between 1 and 12.</param>
    /// <exception cref="DomainValidationException">The year or month is outside the supported range.</exception>
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

    /// <summary>Gets the calendar year.</summary>
    public int Year { get; }

    /// <summary>Gets the month number, 1 through 12.</summary>
    public int Month { get; }

    /// <summary>Gets the first day of the month.</summary>
    public DateOnly FirstDay => new(Year, Month, 1);

    /// <summary>Gets the last day of the month.</summary>
    public DateOnly LastDay => new(Year, Month, DateTime.DaysInMonth(Year, Month));

    /// <summary>Creates the <see cref="BudgetMonth"/> that contains the supplied date.</summary>
    /// <param name="date">Any date inside the wanted month.</param>
    /// <returns>The month containing <paramref name="date"/>.</returns>
    public static BudgetMonth FromDate(DateOnly date) => new(date.Year, date.Month);

    /// <summary>Determines whether the supplied date falls inside this month.</summary>
    /// <param name="date">The date to test.</param>
    /// <returns><see langword="true"/> when the date is inside this month.</returns>
    public bool Contains(DateOnly date) => date.Year == Year && date.Month == Month;

    /// <inheritdoc />
    public int CompareTo(BudgetMonth other)
    {
        int yearComparison = Year.CompareTo(other.Year);
        return yearComparison != 0 ? yearComparison : Month.CompareTo(other.Month);
    }

    /// <inheritdoc />
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Year:D4}-{Month:D2}");
}
