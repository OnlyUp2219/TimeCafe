namespace UserProfile.TimeCafe.API.DTOs;

public record CreateProfileDto(string UserId, string FirstName, string LastName, Gender Gender);

public class CreateProfileDtoExample : IExamplesProvider<CreateProfileDto>
{
    public CreateProfileDto GetExamples()
    {
        return new CreateProfileDto(
            UserId: "user-123-abc",
            FirstName: "Иван",
            LastName: "Петров",
            Gender: Gender.Male
        );
    }
}
