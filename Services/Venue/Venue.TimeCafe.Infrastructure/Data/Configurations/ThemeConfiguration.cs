namespace Venue.TimeCafe.Infrastructure.Data.Configurations;

public class ThemeConfiguration : IEntityTypeConfiguration<Theme>
{
    public void Configure(EntityTypeBuilder<Theme> builder)
    {
        builder.ToTable("Themes");

        builder.HasKey(t => t.ThemeId);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Emoji)
            .HasMaxLength(10);

        builder.Property(t => t.Colors)
            .HasColumnType("jsonb");

        builder.HasMany(t => t.Tariffs)
            .WithOne()
            .HasForeignKey(tar => tar.ThemeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
