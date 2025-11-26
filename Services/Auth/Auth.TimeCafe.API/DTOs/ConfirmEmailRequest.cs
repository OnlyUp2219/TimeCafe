namespace Auth.TimeCafe.API.DTOs;

public record ConfirmEmailRequest(string UserId, string Token);

public class ConfirmEmailRequestExample : IExamplesProvider<ConfirmEmailRequest>
{
    public ConfirmEmailRequest GetExamples()
    {
        return new ConfirmEmailRequest(
            UserId: "550e8400-e29b-41d4-a716-446655440000",
            Token: "CfDJ8N1..."
        );
    }
}
