namespace Venue.TimeCafe.Infrastructure.Data.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("Visits");

        builder.HasKey(v => v.VisitId);

        builder.Property(v => v.UserId)
            .IsRequired(false);

        builder.Property(v => v.ResourceId)
            .IsRequired(false);

        builder.Property(v => v.IsFinishRequested)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(v => v.EntryTime)
            .IsRequired();

        builder.Property(v => v.ExitTime)
            .IsRequired(false);

        builder.Property(v => v.CalculatedCost)
            .HasPrecision(18, 2);

        builder.Property(v => v.Status)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(VisitStatus.Pending)
            .HasSentinel(VisitStatus.Pending);

        builder.Property(v => v.ApprovedByUserId)
            .IsRequired(false);

        builder.Property(v => v.ApprovedAt)
            .IsRequired(false);

        builder.Property(v => v.RejectionReason)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(v => v.PlannedMinutes)
            .IsRequired(false);

        builder.Property(v => v.GuestsCount)
            .IsRequired()
            .HasDefaultValue(1);

        builder.HasOne<Tariff>()
            .WithMany(t => t.Visits)
            .HasForeignKey(v => v.TariffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Resource>()
            .WithMany()
            .HasForeignKey(v => v.ResourceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(v => v.UserId);
        builder.HasIndex(v => v.Status);
        builder.HasIndex(v => new { v.UserId, v.Status });
        builder.HasIndex(v => v.EntryTime);
        builder.HasIndex(v => new { v.Status, v.EntryTime });
        builder.HasIndex(v => new { v.UserId, v.EntryTime });
        builder.HasIndex(v => v.UserId)
            .HasFilter("\"Status\" = 3 AND \"UserId\" IS NOT NULL")
            .IsUnique();
    }
}

