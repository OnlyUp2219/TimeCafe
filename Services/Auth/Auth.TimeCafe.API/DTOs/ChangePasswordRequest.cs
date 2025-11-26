namespace Auth.TimeCafe.API.DTOs;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public class ChangePasswordRequestExample : IExamplesProvider<ChangePasswordRequest>
{
    public ChangePasswordRequest GetExamples()
    {
        return new ChangePasswordRequest(
            CurrentPassword: "OldPass123!",
            NewPassword: "NewSecurePass456!"
        );
    }
}
