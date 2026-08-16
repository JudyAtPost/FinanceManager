using Microsoft.Extensions.DependencyInjection;
using PersonalFinance.Application.Budgets;
using PersonalFinance.Application.Categories;
using PersonalFinance.Application.Summaries;
using PersonalFinance.Application.Transactions;

namespace PersonalFinance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<CategoryService>();
        services.AddScoped<TransactionService>();
        services.AddScoped<BudgetService>();
        services.AddScoped<MonthlySummaryService>();

        return services;
    }
}
