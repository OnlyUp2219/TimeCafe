namespace UserProfile.TimeCafe.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {

    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Profile> Profiles { get; set; }
    public DbSet<AdditionalInfo> AdditionalInfos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AdditionalInfo>().ToTable("AdditionalInfo");
        modelBuilder.Entity<Profile>().ToTable("Profiles");

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MiddleName).HasMaxLength(100);
            entity.Property(e => e.PhotoUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<AdditionalInfo>(entity =>
        {
            entity.HasKey(e => e.InfoId);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.InfoText).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.CreatedBy).HasMaxLength(450);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne<Profile>()
                  .WithMany()
                  .HasForeignKey(ai => ai.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

    }
}
