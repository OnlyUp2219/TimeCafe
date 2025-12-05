namespace Auth.TimeCafe.Infrastructure.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
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
}
