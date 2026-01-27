namespace Auth.TimeCafe.Application.CQRS.Account.Commands;

public record ResetPasswordCommand(string Email, string ResetCode, string NewPassword) : IRequest<ResetPasswordResult>;

public record ResetPasswordResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static ResetPasswordResult UserNotFound() =>
        new(false, Code: "UserNotFound", Message: "Некорректная ссылка для сброса пароля", StatusCode: 400);

    public static ResetPasswordResult InvalidCode() =>
        new(false, Code: "InvalidResetCode", Message: "Некорректный код сброса пароля", StatusCode: 400);

    public static ResetPasswordResult Failed(List<ErrorItem>? errors) =>
        new(false, Code: "ResetPasswordFailed", Message: "Не удалось сбросить пароль", StatusCode: 400, Errors: errors);

    public static ResetPasswordResult SuccessResult() =>
        new(true, Message: "Пароль успешно изменён");
}

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный формат email");

        RuleFor(x => x.ResetCode)
            .NotEmpty().WithMessage("Код обязателен");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Новый пароль обязателен");
    }
}

public class ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<ResetPasswordResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return ResetPasswordResult.UserNotFound();

        string token;
        try
        {
            var decodedBytes = WebEncoders.Base64UrlDecode(request.ResetCode);
            token = Encoding.UTF8.GetString(decodedBytes);
        }
        catch
        {
            return ResetPasswordResult.InvalidCode();
        }

        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
        if (!result.Succeeded)
        {
            List<ErrorItem>? errs = [.. result.Errors.Select(e => new ErrorItem(e.Code, e.Description))];
            return ResetPasswordResult.Failed(errs);
        }

        return ResetPasswordResult.SuccessResult();
    }
}
