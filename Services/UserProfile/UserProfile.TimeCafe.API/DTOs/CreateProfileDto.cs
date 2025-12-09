namespace UserProfile.TimeCafe.API.DTOs;

public record CreateProfileDto(string UserId, string FirstName, string LastName, Gender Gender);

public class CreateProfileDtoExample : IExamplesProvider<CreateProfileDto>
{
    public CreateProfileDto GetExamples()
    {
        return new CreateProfileDto(
            UserId: Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479").ToString(),
            FirstName: "Иван",
            LastName: "Петров",
            Gender: Gender.Male
        );
    }
}
