namespace UserProfile.TimeCafe.API.DTOs;

public record DeletePhotoProfileDto(
    [FromRoute] string UserId);

public class DeletePhotoProfileDtoExample : IExamplesProvider<DeletePhotoProfileDto>
{
    public DeletePhotoProfileDto GetExamples()
    {
        return new DeletePhotoProfileDto(
            UserId: Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479").ToString()
        );
    }
}
