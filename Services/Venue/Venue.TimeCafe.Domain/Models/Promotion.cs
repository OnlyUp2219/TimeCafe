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
    public PromotionType Type { get; set; } = PromotionType.Draft;
    public Guid? TariffId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public static Result<Promotion> Create(Guid? promotionId, string name, string description, DateTimeOffset validFrom, DateTimeOffset validTo, bool isActive, PromotionType type, Guid? tariffId = null, decimal? DiscountPercent = null)
    {
        if (type == PromotionType.TariffSpecific && tariffId == null)
            return Result.Fail<Promotion>(new PromotionInvalidTypeError());
        if (type == PromotionType.Global && tariffId != null)
            return Result.Fail<Promotion>(new PromotionInvalidTypeError());

        return Result.Ok(new Promotion
        {
            PromotionId = promotionId ?? Guid.NewGuid(),
            Name = name,
            Description = description,
            DiscountPercent = DiscountPercent,
            ValidFrom = validFrom,
            ValidTo = validTo,
            IsActive = isActive,
            Type = type,
            TariffId = tariffId,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    public static Result<Promotion> Update(Promotion existingPromotion, string? name = null, string? description = null, decimal? DiscountPercent = null, DateTimeOffset? validFrom = null, DateTimeOffset? validTo = null, bool? isActive = null, PromotionType? type = null, Guid? tariffId = null)
    {
        var newType = type ?? existingPromotion.Type;
        var newTariffId = tariffId ?? existingPromotion.TariffId;

        if (newType == PromotionType.TariffSpecific && newTariffId == null)
            return Result.Fail<Promotion>(new PromotionInvalidTypeError());
        if (newType == PromotionType.Global && newTariffId != null)
            return Result.Fail<Promotion>(new PromotionInvalidTypeError());

        return Result.Ok(new Promotion(existingPromotion.PromotionId)
        {
            Name = name ?? existingPromotion.Name,
            Description = description ?? existingPromotion.Description,
            DiscountPercent = DiscountPercent ?? existingPromotion.DiscountPercent,
            ValidFrom = validFrom ?? existingPromotion.ValidFrom,
            ValidTo = validTo ?? existingPromotion.ValidTo,
            IsActive = isActive ?? existingPromotion.IsActive,
            Type = newType,
            TariffId = newTariffId,
            CreatedAt = existingPromotion.CreatedAt
        });
    }

}

