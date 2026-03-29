namespace UserProfile.TimeCafe.Infrastructure.Data.Configurations;

public class AdditionalInfoConfiguration : IEntityTypeConfiguration<AdditionalInfo>
{
    public void Configure(EntityTypeBuilder<AdditionalInfo> builder)
    {
        builder.ToTable("AdditionalInfo");

        builder.HasKey(e => e.InfoId);

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.InfoText)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(450);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasOne<Profile>()
            .WithMany()
            .HasForeignKey(ai => ai.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.UserId, e.CreatedAt });
    }
}
