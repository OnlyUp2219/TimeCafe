namespace Venue.TimeCafe.API.DTOs.Promotion;

public record ActivatePromotionDto(int PromotionId);

public class ActivatePromotionDtoExample : IExamplesProvider<ActivatePromotionDto>
{
    public ActivatePromotionDto GetExamples() =>
        new(PromotionId: 1);
}
