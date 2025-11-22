namespace Auth.TimeCafe.Application.CQRS.Account.Commands;

public record ConfirmEmailCommand(string UserId, string Token) : IRequest<ConfirmEmailResult>;

public record ConfirmEmailResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static ConfirmEmailResult UserNotFound() =>
        new(false, Code: "UserNotFound", Message: "Пользователь не найден", StatusCode: 401);

    public static ConfirmEmailResult EmailAlreadyConfirmed() =>
        new(false, Code: "EmailAlreadyConfirmed", Message: "Email уже подтвержден", StatusCode: 400);

    public static ConfirmEmailResult InvalidTokenFormat() =>
        new(false, Code: "InvalidTokenFormat", Message: "Неверный формат токена", StatusCode: 400);

    public static ConfirmEmailResult InvalidToken() =>
        new(false, Code: "InvalidToken", Message: "Токен недействителен или уже использован", StatusCode: 400);

    public static ConfirmEmailResult ConfirmEmailSuccess() =>
       new(true, Message: "Email подтвержден");
}

public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден");
    }
}

public class ConfirmEmailCommandHandler(
    UserManager<IdentityUser> userManager) : IRequestHandler<ConfirmEmailCommand, ConfirmEmailResult>
{
    private readonly UserManager<IdentityUser> _userManager = userManager;

    public async Task<ConfirmEmailResult> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return ConfirmEmailResult.UserNotFound();
        if (user.EmailConfirmed)
            return ConfirmEmailResult.EmailAlreadyConfirmed();

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        }
        catch
        {
            return ConfirmEmailResult.InvalidTokenFormat();
        }
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
        {
            return ConfirmEmailResult.InvalidToken();
        }
        return ConfirmEmailResult.ConfirmEmailSuccess();
    }
}
