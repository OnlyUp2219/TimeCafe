namespace UserProfile.TimeCafe.API.DTOs;

public record DeleteAdditionalInfoDto(
    [FromRoute] string InfoId);

public class DeleteAdditionalInfoDtoExample : IExamplesProvider<DeleteAdditionalInfoDto>
{
    public DeleteAdditionalInfoDto GetExamples()
    {
        return new DeleteAdditionalInfoDto(
            InfoId: Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479").ToString()
        );
    }
}