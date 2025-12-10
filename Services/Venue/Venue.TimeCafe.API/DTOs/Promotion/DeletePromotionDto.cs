namespace Venue.TimeCafe.API.DTOs.Promotion;

public record DeletePromotionDto(string PromotionId);

public class DeletePromotionDtoExample : IExamplesProvider<DeletePromotionDto>
{
    public DeletePromotionDto GetExamples() =>
        new(PromotionId: Guid.NewGuid().ToString());
}
