using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PersonalFinance.Domain;

namespace PersonalFinance.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>Fixed seed so the generated sample data is reproducible across runs.</summary>
    private const int SampleDataRandomSeed = 12345;

    private const int SampleDataMonthCount = 6;

    /// <summary>
    /// Applies pending migrations and, optionally, seeds sample data.
    /// </summary>
    /// <remarks>
    /// Migrating on startup suits this single-user app: the container can be brought up with nothing
    /// but a database and the schema is ready. A multi-instance deployment would move this into a
    /// separate migration step to avoid concurrent migrators racing each other.
    /// </remarks>
    public static async Task InitializeAsync(
        IServiceProvider services,
        bool seedSampleData = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using AsyncServiceScope scope = services.CreateAsyncScope();

        FinanceDbContext context = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
        ILogger logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DatabaseInitializer));

        logger.LogInformation("Applying database migrations.");
        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

        if (!seedSampleData)
        {
            return;
        }

        bool alreadySeeded = await context.Categories
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);

        if (alreadySeeded)
        {
            return;
        }

        logger.LogInformation("Seeding sample data.");

        Category salary = Category.Create("Salary", TransactionType.Income).Value;
        Category groceries = Category.Create("Groceries", TransactionType.Expense).Value;
        Category rent = Category.Create("Rent", TransactionType.Expense).Value;
        Category leisure = Category.Create("Leisure", TransactionType.Expense).Value;

        context.Categories.AddRange(salary, groceries, rent, leisure);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Seeding sample transactions and budgets for the last {MonthCount} months.", SampleDataMonthCount);
        await SeedSampleDataAsync(context, salary, groceries, rent, leisure, cancellationToken).ConfigureAwait(false);
    }

    private static async Task SeedSampleDataAsync(
        FinanceDbContext context,
        Category salary,
        Category groceries,
        Category rent,
        Category leisure,
        CancellationToken cancellationToken)
    {
        var random = new Random(SampleDataRandomSeed);
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        BudgetMonth currentMonth = BudgetMonth.FromDate(today);

        for (int offset = SampleDataMonthCount - 1; offset >= 0; offset--)
        {
            BudgetMonth month = BudgetMonth.FromDate(currentMonth.FirstDay.AddMonths(-offset));
            SeedMonth(context, month, random, salary, groceries, rent, leisure);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void SeedMonth(
        FinanceDbContext context,
        BudgetMonth month,
        Random random,
        Category salary,
        Category groceries,
        Category rent,
        Category leisure)
    {
        context.Transactions.Add(
            Transaction.Create("Monthly salary", RandomAmount(random, 3000, 3500), month.FirstDay, salary.Id).Value);

        decimal groceriesTotal = SeedCategoryTransactions(
            context, month, random, groceries, "Grocery shopping", count: random.Next(8, 13), minAmount: 15, maxAmount: 80);

        decimal rentTotal = SeedCategoryTransactions(
            context, month, random, rent, "Rent payment", count: 1, minAmount: 900, maxAmount: 1200);

        decimal leisureTotal = SeedCategoryTransactions(
            context, month, random, leisure, "Leisure activity", count: random.Next(4, 9), minAmount: 10, maxAmount: 120);

        SeedBudget(context, month, groceries.Id, groceriesTotal, random);
        SeedBudget(context, month, rent.Id, rentTotal, random);
        SeedBudget(context, month, leisure.Id, leisureTotal, random);
    }

    private static decimal SeedCategoryTransactions(
        FinanceDbContext context,
        BudgetMonth month,
        Random random,
        Category category,
        string description,
        int count,
        int minAmount,
        int maxAmount)
    {
        decimal total = 0m;

        for (int i = 0; i < count; i++)
        {
            decimal amount = RandomAmount(random, minAmount, maxAmount);
            DateOnly date = month.FirstDay.AddDays(random.Next(0, month.LastDay.Day));

            context.Transactions.Add(Transaction.Create(description, amount, date, category.Id).Value);
            total += amount;
        }

        return total;
    }

    /// <summary>
    /// Seeds a budget for <paramref name="categoryId"/> around <paramref name="actualSpend"/>, occasionally
    /// setting the limit below the actual spend so the "over budget" state has sample data too.
    /// </summary>
    private static void SeedBudget(FinanceDbContext context, BudgetMonth month, Guid categoryId, decimal actualSpend, Random random)
    {
        double factor = random.NextDouble() switch
        {
            < 0.3 => random.NextDouble() * 0.15 + 0.8,  // 0.80 - 0.95: intentionally over budget
            _ => random.NextDouble() * 0.5 + 1.05        // 1.05 - 1.55: comfortably within budget
        };

        decimal limit = Math.Max(1m, Money.Round(actualSpend * (decimal)factor));

        context.Budgets.Add(Budget.Create(categoryId, month, limit).Value);
    }

    private static decimal RandomAmount(Random random, int minAmount, int maxAmount) =>
        Money.Round((decimal)(random.NextDouble() * (maxAmount - minAmount) + minAmount));
}
