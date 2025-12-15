namespace Venue.TimeCafe.API.DTOs.Promotion;

public record ActivatePromotionDto(string PromotionId);

public class ActivatePromotionDtoExample : IExamplesProvider<ActivatePromotionDto>
{
    public ActivatePromotionDto GetExamples() =>
        new(PromotionId: "a1111111-1111-1111-1111-111111111111");
}
