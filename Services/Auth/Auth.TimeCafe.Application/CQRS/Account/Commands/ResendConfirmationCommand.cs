namespace Auth.TimeCafe.Application.CQRS.Account.Commands;

public record ResendConfirmationCommand(string Email, bool SendEmail = true) : IRequest<ResendConfirmationResult>;

public record ResendConfirmationResult(bool Success, string? CallbackUrl = null, string? Message = null, string? Error = null);

public class ResendConfirmationCommandHandler(
    UserManager<IdentityUser> userManager,
    IEmailSender<IdentityUser> emailSender,
    IOptions<PostmarkOptions> postmarkOptions) : IRequestHandler<ResendConfirmationCommand, ResendConfirmationResult>
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly IEmailSender<IdentityUser> _emailSender = emailSender;
    private readonly PostmarkOptions _postmarkOptions = postmarkOptions.Value;

    public async Task<ResendConfirmationResult> Handle(ResendConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return new ResendConfirmationResult(false, Error: "Email не найден");
        if (user.EmailConfirmed)
            return new ResendConfirmationResult(false, Error: "Email уже подтвержден");
        if (string.IsNullOrWhiteSpace(_postmarkOptions.FrontendBaseUrl))
            return new ResendConfirmationResult(false, Error: "FrontendBaseUrl не настроен");
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var callbackUrl = $"{_postmarkOptions.FrontendBaseUrl}/confirm-email?userId={user.Id}&token={encodedToken}";
        
        if (request.SendEmail)
        {
            await _emailSender.SendConfirmationLinkAsync(user, request.Email, callbackUrl);
            return new ResendConfirmationResult(true, Message: "Письмо отправлено");
        }
        
        return new ResendConfirmationResult(true, CallbackUrl: callbackUrl);
    }
}
