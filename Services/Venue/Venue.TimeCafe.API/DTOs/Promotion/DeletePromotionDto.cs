namespace Venue.TimeCafe.API.DTOs.Promotion;

public record DeletePromotionDto(string PromotionId);

public class DeletePromotionDtoExample : IExamplesProvider<DeletePromotionDto>
{
    public DeletePromotionDto GetExamples() =>
        new(PromotionId: "a1111111-1111-1111-1111-111111111111");
}
