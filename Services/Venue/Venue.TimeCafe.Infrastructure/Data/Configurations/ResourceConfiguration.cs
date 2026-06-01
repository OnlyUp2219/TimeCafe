namespace Venue.TimeCafe.Infrastructure.Data.Configurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("Resources");

        builder.HasKey(r => r.ResourceId);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Capacity)
            .IsRequired();

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne<ResourceGroup>()
            .WithMany()
            .HasForeignKey(r => r.ResourceGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
