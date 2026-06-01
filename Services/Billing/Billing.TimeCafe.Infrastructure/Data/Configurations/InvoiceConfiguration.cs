namespace Billing.TimeCafe.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.InvoiceId);

        builder.Property(i => i.InvoiceId)
            .IsRequired();

        builder.Property(i => i.UserId)
            .IsRequired(false);

        builder.Property(i => i.VisitId)
            .IsRequired();

        builder.Property(i => i.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(InvoiceStatus.Pending)
            .HasSentinel(InvoiceStatus.Pending);

        builder.Property(i => i.PaymentMethod)
            .HasConversion<int>()
            .IsRequired(false);

        builder.Property(i => i.StripeSessionId)
            .HasMaxLength(250)
            .IsRequired(false);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.PaidAt)
            .IsRequired(false);

        builder.HasIndex(i => i.UserId);
        builder.HasIndex(i => i.VisitId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.CreatedAt);
    }
}
