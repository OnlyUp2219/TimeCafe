using Billing.TimeCafe.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.TimeCafe.Infrastructure.Data.Configurations;

public class BalanceConfiguration : IEntityTypeConfiguration<Balance>
{
    public void Configure(EntityTypeBuilder<Balance> builder)
    {
        builder.ToTable("Balances");

        builder.HasKey(b => b.UserId);

        builder.Property(b => b.UserId)
            .IsRequired();

        builder.Property(b => b.CurrentBalance)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.TotalDeposited)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.TotalSpent)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.Debt)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.LastUpdated)
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        // Индексы для поиска должников
        builder.HasIndex(b => b.Debt)
            .HasFilter("\"Debt\" > 0");

        builder.HasIndex(b => b.LastUpdated);
    }
}
