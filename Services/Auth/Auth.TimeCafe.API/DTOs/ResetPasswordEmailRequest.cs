namespace Auth.TimeCafe.API.DTOs;

public record ResetPasswordEmailRequest(string Email);

public class ResetPasswordEmailRequestExample : IExamplesProvider<ResetPasswordEmailRequest>
{
    public ResetPasswordEmailRequest GetExamples()
    {
        return new ResetPasswordEmailRequest(
            Email: "user@example.com"
        );
    }
}
