namespace UserProfile.TimeCafe.API.DTOs;

public record DeleteProfileDto(
    [FromRoute] string UserId);

public class DeleteProfileDtoExample : IExamplesProvider<DeleteProfileDto>
{
    public DeleteProfileDto GetExamples()
    {
        return new DeleteProfileDto(
            UserId: Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479").ToString()
        );
    }
}
