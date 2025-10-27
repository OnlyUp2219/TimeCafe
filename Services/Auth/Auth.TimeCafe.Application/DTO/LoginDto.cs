namespace Auth.TimeCafe.Application.DTO;

public record LoginDto(string Email, string Password);

public class LoginDtoExample : IExamplesProvider<LoginDto>
{
    public LoginDto GetExamples() =>
        new("user@example.com", "123456");
}