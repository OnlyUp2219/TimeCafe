namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record RegisterUserCommand(string Username, string Email, string Password, bool SendEmail = true) : IRequest<RegisterUserResult>;

public record RegisterUserResult(bool Success, string? CallbackUrl = null, string? Message = null, IEnumerable<object>? Errors = null);

public class RegisterUserCommandHandler(
    UserManager<IdentityUser> userManager,
    IEmailSender<IdentityUser> emailSender,
    IOptions<PostmarkOptions> postmarkOptions) : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly IEmailSender<IdentityUser> _emailSender = emailSender;
    private readonly PostmarkOptions _postmarkOptions = postmarkOptions.Value;

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new IdentityUser { UserName = request.Username, Email = request.Email, EmailConfirmed = false };
        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return new RegisterUserResult(false, Errors: createResult.Errors.Select(e => new { code = e.Code, description = e.Description }).ToList());
        }
        if (string.IsNullOrWhiteSpace(_postmarkOptions.FrontendBaseUrl))
            return new RegisterUserResult(false, Errors: [new { code = "Configuration", description = "FrontendBaseUrl is not configured" }]);
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var callbackUrl = $"{_postmarkOptions.FrontendBaseUrl}/confirm-email?userId={user.Id}&token={encodedToken}";
        if (request.SendEmail)
        {
            try
            {
                await _emailSender.SendConfirmationLinkAsync(user, request.Email, callbackUrl);
                return new RegisterUserResult(true, Message: "Письмо отправлено");
            }
            catch (Exception)
            {
                return new RegisterUserResult(false, Errors: [new { code = "EmailSendFailed", description = "Ошибка при отправке письма" }]);
            }
        }
        return new RegisterUserResult(true, CallbackUrl: callbackUrl);
    }
}
