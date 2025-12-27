namespace UserProfile.TimeCafe.API.DTOs;

public record CreateEmptyProfileDto(
    [FromRoute] string UserId);

public class CreateEmptyProfileDtoExample : IExamplesProvider<CreateEmptyProfileDto>
{
    public CreateEmptyProfileDto GetExamples()
    {
        return new CreateEmptyProfileDto(
            UserId: Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479").ToString()
        );
    }
}
