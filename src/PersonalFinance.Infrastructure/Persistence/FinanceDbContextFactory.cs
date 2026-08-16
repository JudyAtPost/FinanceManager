using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PersonalFinance.Infrastructure.Persistence;

public sealed class FinanceDbContextFactory : IDesignTimeDbContextFactory<FinanceDbContext>
{
    private const string FallbackConnectionString =
        "Host=localhost;Port=5432;Database=personalfinance;Username=postgres;Password=postgres";

    /// <inheritdoc />
    public FinanceDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PersonalFinance")
            ?? FallbackConnectionString;

        DbContextOptions<FinanceDbContext> options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new FinanceDbContext(options);
    }
}
