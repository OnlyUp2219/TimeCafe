namespace Venue.TimeCafe.API.DTOs.Promotion;

public record UpdatePromotionDto(int PromotionId, string Name, string Description, decimal? DiscountPercent, DateTime ValidFrom, DateTime ValidTo, bool IsActive);

public class UpdatePromotionDtoExample : IExamplesProvider<UpdatePromotionDto>
{
    public UpdatePromotionDto GetExamples() =>
        new(PromotionId: 1, Name: "Счастливые часы", Description: "Скидка 25% на все услуги", DiscountPercent: 25m, ValidFrom: DateTime.UtcNow, ValidTo: DateTime.UtcNow.AddDays(60), IsActive: true);
}
