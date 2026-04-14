using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.TimeCafe.Infrastructure.Data.Configurations;

public class VisitBillingSagaStateConfiguration : IEntityTypeConfiguration<VisitBillingSagaState>
{
    public void Configure(EntityTypeBuilder<VisitBillingSagaState> builder)
    {
        builder.ToTable("VisitBillingSagas");

        builder.HasKey(x => x.VisitId);

        builder.Property(x => x.VisitId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.TransactionId)
            .IsRequired(false);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.FailureReason)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .IsRequired(false);

        builder.Property(x => x.CompensatedAt)
            .IsRequired(false);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.UpdatedAt);
    }
}