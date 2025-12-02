namespace Venue.TimeCafe.API.DTOs.Promotion;

public record GetActivePromotionsByDateDto(DateTime Date);

public class GetActivePromotionsByDateDtoExample : IExamplesProvider<GetActivePromotionsByDateDto>
{
    public GetActivePromotionsByDateDto GetExamples() =>
        new(Date: DateTime.UtcNow);
}
