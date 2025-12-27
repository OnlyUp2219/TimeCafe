namespace UserProfile.TimeCafe.API.DTOs;

public record GetProfileByIdDto(
    [FromRoute] string UserId);

public class GetProfileByIdDtoExample : IExamplesProvider<GetProfileByIdDto>
{
    public GetProfileByIdDto GetExamples()
    {
        return new GetProfileByIdDto(
            UserId: Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479").ToString()
        );
    }
}
