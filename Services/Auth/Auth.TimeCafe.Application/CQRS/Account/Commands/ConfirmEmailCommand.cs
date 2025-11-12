namespace Auth.TimeCafe.Application.CQRS.Account.Commands;

public record ConfirmEmailCommand(string UserId, string Token) : IRequest<ConfirmEmailResult>;

public record ConfirmEmailResult(bool Success, string? Message = null, string? Error = null);

public class ConfirmEmailCommandHandler(
    UserManager<IdentityUser> userManager) : IRequestHandler<ConfirmEmailCommand, ConfirmEmailResult>
{
    private readonly UserManager<IdentityUser> _userManager = userManager;

    public async Task<ConfirmEmailResult> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return new ConfirmEmailResult(false, Error: "Пользователь не найден");
        if (user.EmailConfirmed)
            return new ConfirmEmailResult(true, Message: "Email уже подтвержден");
        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        }
        catch
        {
            return new ConfirmEmailResult(false, Error: "Неверный токен");
        }
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
        {
            return new ConfirmEmailResult(false, Error: "Неверный или истекший токен");
        }
        return new ConfirmEmailResult(true, Message: "Email подтвержден");
    }
}
