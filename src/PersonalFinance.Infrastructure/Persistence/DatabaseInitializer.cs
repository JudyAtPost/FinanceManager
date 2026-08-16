using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PersonalFinance.Domain;

namespace PersonalFinance.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using AsyncServiceScope scope = services.CreateAsyncScope();

        FinanceDbContext context = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
        ILogger logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DatabaseInitializer));

        logger.LogInformation("Applying database migrations.");
        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

        if (await context.Categories.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        logger.LogInformation("Seeding default categories.");
        context.Categories.AddRange(
            Category.Create("Salary", TransactionType.Income),
            Category.Create("Groceries", TransactionType.Expense),
            Category.Create("Rent", TransactionType.Expense),
            Category.Create("Leisure", TransactionType.Expense));

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
