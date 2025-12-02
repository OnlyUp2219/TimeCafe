namespace Venue.TimeCafe.API.DTOs.Promotion;

public record DeactivatePromotionDto(int PromotionId);

public class DeactivatePromotionDtoExample : IExamplesProvider<DeactivatePromotionDto>
{
    public DeactivatePromotionDto GetExamples() =>
        new(PromotionId: 1);
}
