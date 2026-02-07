namespace Auth.TimeCafe.API.DTOs;

public record ChangeEmailRequest(string NewEmail);

public class ChangeEmailRequestExample : IExamplesProvider<ChangeEmailRequest>
{
    public ChangeEmailRequest GetExamples()
    {
        return new ChangeEmailRequest(
            NewEmail: "new.email@example.com"
        );
    }
}
