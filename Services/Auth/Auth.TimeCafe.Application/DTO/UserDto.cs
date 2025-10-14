using Auth.TimeCafe.Application.DTO;

namespace Auth.TimeCafe.Application.DTO
{
    public record UserDto(
        string FirstName,
        string LastName,
        string? MiddleName,
        string Email,
        bool EmailConfirmed,
        int? GenderId,
        DateTime? BirthDate,
        string? PhoneNumber,
        bool? PhoneNumberConfirmed,
        byte[]? Photo,
        string? AccessCardNumber
        );
}

public class UserDtoExamples : IExamplesProvider<UserDto>
{
    public UserDto GetExamples() =>
    new UserDto(
        "Daniil",
        "Klimenkov",
        "Andreevich",
        "user@example.com",
        false,
        1,
        DateTime.Now,
        "+375 29 7143237",
        false,
        null,
        null
    );
}