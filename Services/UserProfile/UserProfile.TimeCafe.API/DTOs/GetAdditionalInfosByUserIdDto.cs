namespace UserProfile.TimeCafe.API.DTOs;

public record GetAdditionalInfosByUserIdDto(
    string UserId);

public class GetAdditionalInfosByUserIdDtoExample : IExamplesProvider<GetAdditionalInfosByUserIdDto>
{
    public GetAdditionalInfosByUserIdDto GetExamples()
    {
        return new GetAdditionalInfosByUserIdDto(
            UserId: Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479").ToString()
        );
    }
}
