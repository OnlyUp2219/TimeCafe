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
    }
}
