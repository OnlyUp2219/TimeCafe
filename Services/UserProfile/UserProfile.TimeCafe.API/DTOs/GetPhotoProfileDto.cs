namespace UserProfile.TimeCafe.API.DTOs;

public record GetPhotoProfileDto(
    [FromRoute] string UserId);

public class GetPhotoProfileDtoExample : IExamplesProvider<GetPhotoProfileDto>
{
    public GetPhotoProfileDto GetExamples()
    {
        return new GetPhotoProfileDto(
            UserId: Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479").ToString()
        );
    }
}
