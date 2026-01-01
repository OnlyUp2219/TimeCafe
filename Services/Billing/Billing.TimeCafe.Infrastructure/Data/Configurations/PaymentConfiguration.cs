using Billing.TimeCafe.Domain.Enums;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.TimeCafe.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.PaymentId);

        builder.Property(p => p.PaymentId)
            .IsRequired();

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.PaymentMethod)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.ExternalPaymentId)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(p => p.Status)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(PaymentStatus.Pending)
            .HasSentinel(PaymentStatus.Pending);

        builder.Property(p => p.TransactionId)
            .IsRequired(false);

        builder.Property(p => p.ExternalData)
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.CompletedAt)
            .IsRequired(false);

        builder.Property(p => p.ErrorMessage)
            .HasMaxLength(1000)
            .IsRequired(false);

        // Индексы
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.ExternalPaymentId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedAt);

        builder.HasOne<Balance>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Transaction>()
            .WithMany()
            .HasForeignKey(p => p.TransactionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
