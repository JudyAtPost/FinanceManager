using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="Category"/> to the categories table.
/// </summary>
public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("categories");
        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id).ValueGeneratedNever();

        builder.Property(category => category.Name)
            .HasMaxLength(Category.MaxNameLength)
            .IsRequired();

        builder.Property(category => category.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(category => category.Name).IsUnique();
    }
}
