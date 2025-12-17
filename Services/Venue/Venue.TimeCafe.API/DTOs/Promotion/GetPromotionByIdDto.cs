namespace Venue.TimeCafe.API.DTOs.Promotion;

public record GetPromotionByIdDto(string PromotionId);

public class GetPromotionByIdDtoExample : IExamplesProvider<GetPromotionByIdDto>
{
    public GetPromotionByIdDto GetExamples() =>
        new(PromotionId: "a1111111-1111-1111-1111-111111111111");
}
