namespace Auth.TimeCafe.API.DTOs;

public record ResendConfirmationRequest(string Email);

public class ResendConfirmationRequestExample : IExamplesProvider<ResendConfirmationRequest>
{
    public ResendConfirmationRequest GetExamples()
    {
        return new ResendConfirmationRequest(
            Email: "user@example.com"
        );
    }
}
