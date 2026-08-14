using Microsoft.Extensions.DependencyInjection;
using PersonalFinance.Application.Budgets;
using PersonalFinance.Application.Categories;
using PersonalFinance.Application.Summaries;
using PersonalFinance.Application.Transactions;

namespace PersonalFinance.Application;

/// <summary>
/// Registers the application layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Adds the use-case services of the application layer.</summary>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The same service collection, for chaining.</returns>
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
