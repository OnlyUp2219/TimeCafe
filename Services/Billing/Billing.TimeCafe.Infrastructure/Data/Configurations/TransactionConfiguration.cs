using Billing.TimeCafe.Domain.Enums;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.TimeCafe.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.TransactionId);

        builder.Property(t => t.TransactionId)
            .IsRequired();

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.Source)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.SourceId)
            .IsRequired(false);

        builder.Property(t => t.Status)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(TransactionStatus.Completed)
            .HasSentinel(TransactionStatus.Pending);

        builder.Property(t => t.Comment)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.BalanceAfter)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.SourceId);
        builder.HasIndex(t => new { t.Source, t.SourceId })
            .HasDatabaseName("IX_Transactions_Source_SourceId");
        builder.HasIndex(t => t.CreatedAt);
        builder.HasIndex(t => t.Type);

        builder.HasOne<Balance>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
