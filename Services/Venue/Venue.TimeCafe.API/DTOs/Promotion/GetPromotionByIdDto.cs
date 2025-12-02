namespace Venue.TimeCafe.API.DTOs.Promotion;

public record GetPromotionByIdDto(int PromotionId);

public class GetPromotionByIdDtoExample : IExamplesProvider<GetPromotionByIdDto>
{
    public GetPromotionByIdDto GetExamples() =>
        new(PromotionId: 1);
}
