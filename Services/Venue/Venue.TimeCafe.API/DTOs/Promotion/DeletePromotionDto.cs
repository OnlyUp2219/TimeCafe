namespace Venue.TimeCafe.API.DTOs.Promotion;

public record DeletePromotionDto(int PromotionId);

public class DeletePromotionDtoExample : IExamplesProvider<DeletePromotionDto>
{
    public DeletePromotionDto GetExamples() =>
        new(PromotionId: 1);
}
