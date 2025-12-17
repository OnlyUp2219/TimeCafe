namespace Venue.TimeCafe.API.DTOs.Promotion;

public record CreatePromotionDto(string Name, string Description, decimal? DiscountPercent, DateTimeOffset ValidFrom, DateTimeOffset ValidTo, bool IsActive);

public class CreatePromotionDtoExample : IExamplesProvider<CreatePromotionDto>
{
    public CreatePromotionDto GetExamples() =>
        new(Name: "Счастливые часы", Description: "Скидка 20% на все услуги", DiscountPercent: 20m, ValidFrom: DateTimeOffset.UtcNow, ValidTo: DateTimeOffset.UtcNow.AddDays(30), IsActive: true);
}
