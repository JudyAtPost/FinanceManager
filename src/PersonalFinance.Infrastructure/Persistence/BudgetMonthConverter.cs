using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PersonalFinance.Domain;

namespace PersonalFinance.Infrastructure.Persistence;

/// <summary>
/// Stores a <see cref="BudgetMonth"/> as a single sortable integer in the form yyyyMM.
/// </summary>
public sealed class BudgetMonthConverter : ValueConverter<BudgetMonth, int>
{
    /// <summary>Initializes a new instance of the <see cref="BudgetMonthConverter"/> class.</summary>
    public BudgetMonthConverter()
        : base(month => (month.Year * 100) + month.Month, value => new BudgetMonth(value / 100, value % 100))
    {
    }
}
