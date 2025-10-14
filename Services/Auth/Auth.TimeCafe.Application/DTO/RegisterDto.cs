namespace Auth.TimeCafe.Application.DTO;

public record RegisterDto(string Username, string Email, string Password);

public class RegisterDtoExample : IExamplesProvider<RegisterDto>
{
    public RegisterDto GetExamples() =>
        new RegisterDto("test_user", "user@example.com", "qwert1");
}
