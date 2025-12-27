namespace UserProfile.TimeCafe.API.DTOs;

public record GetProfilesPageDto(
    [FromQuery] int PageNumber = 1,
    [FromQuery] int PageSize = 10);

public class GetProfilesPageDtoExample : IExamplesProvider<GetProfilesPageDto>
{
    public GetProfilesPageDto GetExamples()
    {
        return new GetProfilesPageDto(
            PageNumber: 1,
            PageSize: 10
        );
    }
}
