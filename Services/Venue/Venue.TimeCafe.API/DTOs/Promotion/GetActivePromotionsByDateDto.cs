namespace Venue.TimeCafe.API.DTOs.Promotion;

public record GetActivePromotionsByDateDto(DateTimeOffset Date);

public class GetActivePromotionsByDateDtoExample : IExamplesProvider<GetActivePromotionsByDateDto>
{
    public GetActivePromotionsByDateDto GetExamples() =>
        new(Date: DateTimeOffset.UtcNow);
}
