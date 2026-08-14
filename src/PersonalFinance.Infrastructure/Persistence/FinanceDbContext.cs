using Microsoft.EntityFrameworkCore;
using PersonalFinance.Domain;

namespace PersonalFinance.Infrastructure.Persistence;

/// <summary>
/// The EF Core context holding categories, transactions, and budgets.
/// </summary>
public sealed class FinanceDbContext : DbContext
{
    /// <summary>Initializes a new instance of the <see cref="FinanceDbContext"/> class.</summary>
    /// <param name="options">The context options supplied by dependency injection.</param>
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options)
        : base(options)
    {
    }

    /// <summary>Gets the categories transactions and budgets are grouped under.</summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>Gets the recorded income and expenses.</summary>
    public DbSet<Transaction> Transactions => Set<Transaction>();

    /// <summary>Gets the monthly spending limits per category.</summary>
    public DbSet<Budget> Budgets => Set<Budget>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
