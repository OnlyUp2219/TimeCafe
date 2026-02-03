namespace UserProfile.TimeCafe.API.DTOs;

public record UpdateProfileDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? PhotoUrl,
    DateOnly? BirthDate,
    Gender Gender);

public class UpdateProfileDtoExample : IExamplesProvider<UpdateProfileDto>
{
    public UpdateProfileDto GetExamples()
    {
        return new UpdateProfileDto(
            UserId: Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
            FirstName: "Иван",
            LastName: "Петров",
            MiddleName: "Иванович",
            PhotoUrl: "https://example.com/photos/user123.jpg",
            BirthDate: new DateOnly(1990, 5, 15),
            Gender: Gender.Male
        );
    }
}
