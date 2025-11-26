namespace UserProfile.TimeCafe.API.DTOs;

public record UpdateProfileDto(
    string UserId,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? AccessCardNumber,
    string? PhotoUrl,
    DateOnly? BirthDate,
    Gender Gender,
    ProfileStatus ProfileStatus,
    string? BanReason);

public class UpdateProfileDtoExample : IExamplesProvider<UpdateProfileDto>
{
    public UpdateProfileDto GetExamples()
    {
        return new UpdateProfileDto(
            UserId: "user-123-abc",
            FirstName: "Иван",
            LastName: "Петров",
            MiddleName: "Иванович",
            AccessCardNumber: "CARD-001",
            PhotoUrl: "https://example.com/photos/user123.jpg",
            BirthDate: new DateOnly(1990, 5, 15),
            Gender: Gender.Male,
            ProfileStatus: ProfileStatus.Completed,
            BanReason: null
        );
    }
}
