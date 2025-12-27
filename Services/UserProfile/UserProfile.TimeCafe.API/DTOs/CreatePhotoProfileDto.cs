namespace UserProfile.TimeCafe.API.DTOs;

public record CreatePhotoProfileDto(
    [FromRoute] string UserId,
    [FromForm] IFormFile File);

public class CreatePhotoProfileDtoExample : IExamplesProvider<CreatePhotoProfileDto>
{
    public CreatePhotoProfileDto GetExamples()
    {
        return new CreatePhotoProfileDto(
            UserId: Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479").ToString(),
            File: null!
        );
    }
}
