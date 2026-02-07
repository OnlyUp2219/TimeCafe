namespace Auth.TimeCafe.API.DTOs;

public record ConfirmChangeEmailRequest(string UserId, string NewEmail, string Token);

public class ConfirmChangeEmailRequestExample : IExamplesProvider<ConfirmChangeEmailRequest>
{
    public ConfirmChangeEmailRequest GetExamples()
    {
        return new ConfirmChangeEmailRequest(
            UserId: "550e8400-e29b-41d4-a716-446655440000",
            NewEmail: "new.email@example.com",
            Token: "CfDJ8N1..."
        );
    }
}
