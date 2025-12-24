namespace Venue.TimeCafe.Infrastructure.Data.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("Visits");

        builder.HasKey(v => v.VisitId);

        builder.Property(v => v.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(v => v.EntryTime)
            .IsRequired();

        builder.Property(v => v.ExitTime)
            .IsRequired(false);

        builder.Property(v => v.CalculatedCost)
            .HasPrecision(18, 2);

        builder.Property(v => v.Status)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(VisitStatus.Active)
            .HasSentinel(VisitStatus.Active);

        builder.HasOne<Tariff>()
            .WithMany(t => t.Visits)
            .HasForeignKey(v => v.TariffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.UserId);
        builder.HasIndex(v => v.Status);
        builder.HasIndex(v => new { v.UserId, v.Status });
        builder.HasIndex(v => v.EntryTime);
    }
}
