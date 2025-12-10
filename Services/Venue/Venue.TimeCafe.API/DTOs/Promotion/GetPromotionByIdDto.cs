namespace Venue.TimeCafe.API.DTOs.Promotion;

public record GetPromotionByIdDto(string PromotionId);

public class GetPromotionByIdDtoExample : IExamplesProvider<GetPromotionByIdDto>
{
    public GetPromotionByIdDto GetExamples() =>
        new(PromotionId: Guid.NewGuid().ToString());
}
