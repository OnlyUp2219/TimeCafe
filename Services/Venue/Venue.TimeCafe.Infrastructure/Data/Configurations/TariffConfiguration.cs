namespace Venue.TimeCafe.Infrastructure.Data.Configurations;

public class TariffConfiguration : IEntityTypeConfiguration<Tariff>
{
    public void Configure(EntityTypeBuilder<Tariff> builder)
    {
        builder.ToTable("Tariffs");

        builder.HasKey(t => t.TariffId);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.PricePerMinute)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.BillingType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.IsActive)
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(t => t.LastModified)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(t => t.Theme)
            .WithMany(th => th.Tariffs)
            .HasForeignKey(t => t.ThemeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Visits)
            .WithOne(v => v.Tariff)
            .HasForeignKey(v => v.TariffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.IsActive);
        builder.HasIndex(t => t.BillingType);
    }
}
