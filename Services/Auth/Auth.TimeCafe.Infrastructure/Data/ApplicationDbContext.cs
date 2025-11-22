namespace Auth.TimeCafe.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(x => x.Token)
                    .IsUnique();

                entity.HasIndex(x => new { x.UserId, x.IsRevoked })
                    .HasDatabaseName("IX_RefreshTokens_UserId_IsRevoked");

                entity.HasIndex(x => x.Expires)
                    .HasDatabaseName("IX_RefreshTokens_Expires");
            });
        }
    }
}
