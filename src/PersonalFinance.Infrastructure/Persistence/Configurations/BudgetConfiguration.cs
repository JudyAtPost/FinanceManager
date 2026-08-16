using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

public sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("budgets");
        builder.HasKey(budget => budget.Id);

        builder.Property(budget => budget.Id).ValueGeneratedNever();

        builder.Property(budget => budget.Month)
            .HasConversion<BudgetMonthConverter>()
            .HasColumnName("month")
            .IsRequired();

        builder.Property(budget => budget.Limit)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.HasOne(budget => budget.Category)
            .WithMany()
            .HasForeignKey(budget => budget.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(budget => new { budget.CategoryId, budget.Month }).IsUnique();
    }
}
