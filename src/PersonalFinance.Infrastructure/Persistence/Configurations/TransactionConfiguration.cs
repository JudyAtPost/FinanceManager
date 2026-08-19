using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("transactions");
        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Id).ValueGeneratedNever();

        builder.Property(transaction => transaction.Description)
            .HasMaxLength(Transaction.MaxDescriptionLength)
            .IsRequired();

        builder.Property(transaction => transaction.Amount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(transaction => transaction.Date)
            .HasColumnType("date")
            .IsRequired();

        builder.Ignore(transaction => transaction.Type);

        builder.HasOne(transaction => transaction.Category)
            .WithMany()
            .HasForeignKey(transaction => transaction.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Listing and monthly summaries are always date-driven.
        builder.HasIndex(transaction => transaction.Date);
    }
}
