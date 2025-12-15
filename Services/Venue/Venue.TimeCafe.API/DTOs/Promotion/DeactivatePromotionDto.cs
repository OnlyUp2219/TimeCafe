namespace Venue.TimeCafe.API.DTOs.Promotion;

public record DeactivatePromotionDto(string PromotionId);

public class DeactivatePromotionDtoExample : IExamplesProvider<DeactivatePromotionDto>
{
    public DeactivatePromotionDto GetExamples() =>
        new(PromotionId: "a1111111-1111-1111-1111-111111111111");
}
