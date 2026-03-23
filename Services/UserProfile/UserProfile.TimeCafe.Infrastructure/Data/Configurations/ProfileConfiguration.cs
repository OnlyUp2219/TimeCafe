namespace UserProfile.TimeCafe.Infrastructure.Data.Configurations;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("Profiles");

        builder.HasKey(e => e.UserId);

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.MiddleName)
            .HasMaxLength(100);

        builder.Property(e => e.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.Gender)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(e => e.ProfileStatus)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(e => e.BanReason)
            .HasMaxLength(500);

        builder.HasIndex(e => e.CreatedAt);
    }
}
