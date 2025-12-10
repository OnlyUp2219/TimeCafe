namespace Venue.TimeCafe.API.DTOs.Promotion;

public record ActivatePromotionDto(string PromotionId);

public class ActivatePromotionDtoExample : IExamplesProvider<ActivatePromotionDto>
{
    public ActivatePromotionDto GetExamples() =>
        new(PromotionId: Guid.NewGuid().ToString());
}
