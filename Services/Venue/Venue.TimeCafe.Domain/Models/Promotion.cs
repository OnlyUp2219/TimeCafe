namespace Venue.TimeCafe.Domain.Models;

public class Promotion
{
    public Promotion()
    {
        PromotionId = Guid.NewGuid();
    }

    public Promotion(Guid promotionId)
    {
        PromotionId = promotionId;
    }

    public Guid PromotionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? DiscountPercent { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public static Promotion Create(Guid? promotionId, string name, string description, DateTimeOffset validFrom, DateTimeOffset validTo, bool isActive, decimal? discountPercent = null)
    {
        return new Promotion
        {
            PromotionId = promotionId ?? Guid.NewGuid(),
            Name = name,
            Description = description,
            DiscountPercent = discountPercent,
            ValidFrom = validFrom,
            ValidTo = validTo,
            IsActive = isActive,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public static Promotion Update(Promotion existingPromotion, string? name = null, string? description = null, decimal? discountPercent = null, DateTimeOffset? validFrom = null, DateTimeOffset? validTo = null, bool? isActive = null)
    {
        return new Promotion(existingPromotion.PromotionId)
        {
            Name = name ?? existingPromotion.Name,
            Description = description ?? existingPromotion.Description,
            DiscountPercent = discountPercent ?? existingPromotion.DiscountPercent,
            ValidFrom = validFrom ?? existingPromotion.ValidFrom,
            ValidTo = validTo ?? existingPromotion.ValidTo,
            IsActive = isActive ?? existingPromotion.IsActive,
            CreatedAt = existingPromotion.CreatedAt
        };
    }

}
