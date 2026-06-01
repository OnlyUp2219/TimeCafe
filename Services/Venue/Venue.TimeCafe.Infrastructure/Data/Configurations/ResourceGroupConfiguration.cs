namespace Venue.TimeCafe.Infrastructure.Data.Configurations;

public class ResourceGroupConfiguration : IEntityTypeConfiguration<ResourceGroup>
{
    public void Configure(EntityTypeBuilder<ResourceGroup> builder)
    {
        builder.ToTable("ResourceGroups");

        builder.HasKey(rg => rg.ResourceGroupId);

        builder.Property(rg => rg.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rg => rg.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(rg => rg.Capacity)
            .IsRequired();

        builder.Property(rg => rg.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
    }
}
