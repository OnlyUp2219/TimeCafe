namespace Venue.TimeCafe.API.DTOs.Promotion;

public record DeactivatePromotionDto(string PromotionId);

public class DeactivatePromotionDtoExample : IExamplesProvider<DeactivatePromotionDto>
{
    public DeactivatePromotionDto GetExamples() =>
        new(PromotionId: Guid.NewGuid().ToString());
}
