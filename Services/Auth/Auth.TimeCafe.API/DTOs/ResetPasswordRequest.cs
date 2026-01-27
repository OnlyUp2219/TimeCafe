namespace Auth.TimeCafe.API.DTOs;

public record ResetPasswordRequest(string Email, string ResetCode, string NewPassword);

public class ResetPasswordRequestExample : IExamplesProvider<ResetPasswordRequest>
{
    public ResetPasswordRequest GetExamples()
    {
        return new ResetPasswordRequest(
            Email: "user@example.com",
            ResetCode: "BASE64URL_CODE_FROM_EMAIL",
            NewPassword: "NewPassword123!");
    }
}
