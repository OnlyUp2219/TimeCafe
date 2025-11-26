namespace Auth.TimeCafe.API.DTOs;

public record LoginDto(string Email, string Password);

public class LoginDtoExample : IExamplesProvider<LoginDto>
{
    public LoginDto GetExamples() =>
        new("user@example.com", "123456");
}
