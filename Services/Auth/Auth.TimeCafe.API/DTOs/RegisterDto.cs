namespace Auth.TimeCafe.API.DTOs;

public record RegisterDto(string Username, string Email, string Password);

public class RegisterDtoExample : IExamplesProvider<RegisterDto>
{
    public RegisterDto GetExamples() =>
        new("test_user", "user@example.com", "qwert1");
}
