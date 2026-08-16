using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PersonalFinance.Domain;

namespace PersonalFinance.Infrastructure.Persistence;

public sealed class BudgetMonthConverter : ValueConverter<BudgetMonth, int>
{
    public BudgetMonthConverter()
        : base(month => (month.Year * 100) + month.Month, value => BudgetMonth.Create(value / 100, value % 100).Value)
    {
    }
}
