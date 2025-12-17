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
            .IsRequired();

        builder.Property(t => t.LastModified)
            .IsRequired();

        builder.HasOne<Theme>()
            .WithMany(th => th.Tariffs)
            .HasForeignKey(t => t.ThemeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Visits)
            .WithOne()
            .HasForeignKey(v => v.TariffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.IsActive);
        builder.HasIndex(t => t.BillingType);
    }
}
