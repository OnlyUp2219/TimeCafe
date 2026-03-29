namespace Auth.TimeCafe.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.ReplacedByToken)
            .HasMaxLength(2048);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Token)
            .IsUnique();

        builder.HasIndex(x => new { x.UserId, x.IsRevoked });

        builder.HasIndex(x => x.Expires);
    }
}
