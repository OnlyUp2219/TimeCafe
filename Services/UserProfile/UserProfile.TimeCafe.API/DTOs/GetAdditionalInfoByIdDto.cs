namespace UserProfile.TimeCafe.API.DTOs;

public record GetAdditionalInfoByIdDto(
    [FromRoute] string InfoId);

public class GetAdditionalInfoByIdDtoExample : IExamplesProvider<GetAdditionalInfoByIdDto>
{
    public GetAdditionalInfoByIdDto GetExamples()
    {
        return new GetAdditionalInfoByIdDto(
            InfoId: Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479").ToString()
        );
    }
}
