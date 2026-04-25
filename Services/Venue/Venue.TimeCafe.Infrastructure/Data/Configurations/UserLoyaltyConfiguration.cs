namespace Venue.TimeCafe.Infrastructure.Data.Configurations;

public class UserLoyaltyConfiguration : IEntityTypeConfiguration<UserLoyalty>
{
    public void Configure(EntityTypeBuilder<UserLoyalty> builder)
    {
        builder.HasKey(x => x.UserId);
        builder.Property(x => x.PersonalDiscountPercent).HasColumnType("decimal(18,2)");
    }
}

