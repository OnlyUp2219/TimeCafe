namespace Venue.TimeCafe.API.DTOs.Promotion;

public record UpdatePromotionDto(string PromotionId, string Name, string Description, decimal? DiscountPercent, DateTimeOffset ValidFrom, DateTimeOffset ValidTo, bool IsActive);

public class UpdatePromotionDtoExample : IExamplesProvider<UpdatePromotionDto>
{
    public UpdatePromotionDto GetExamples() =>
        new(PromotionId: "a1111111-1111-1111-1111-111111111111", Name: "Счастливые часы", Description: "Скидка 25% на все услуги", DiscountPercent: 25m, ValidFrom: DateTimeOffset.UtcNow, ValidTo: DateTimeOffset.UtcNow.AddDays(60), IsActive: true);
}
