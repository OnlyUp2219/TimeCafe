namespace Auth.TimeCafe.Application.CQRS.Account.Commands;
public record ChangePasswordCommand(string UserId, string CurrentPassword, string NewPassword) : IRequest<ChangePasswordResult>;

public record ChangePasswordResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    int? RefreshTokensRevoked = null) : ICqrsResultV2
{
    public static ChangePasswordResult UserNotFound() =>
        new(false, Code: "UserNotFound", Message: "Пользователь не найден", StatusCode: 401);

    public static ChangePasswordResult ChangePasswordFailed(List<ErrorItem>? errorItems) =>
        new(false, Code: "ChangePasswordFailed", Message: "Не удалось сменить пароль", StatusCode: 400, 
            Errors: errorItems);

    public static ChangePasswordResult ChangePasswordSuccess(int refreshTokensRevoked) =>
        new(true, Message: "Пароль изменён", RefreshTokensRevoked: refreshTokensRevoked);
}

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Текущий пароль обязателен")
            .NotEqual(x => x.NewPassword).WithMessage("Новый пароль не должен совпадать со старым");

    }
}

public class ChangePasswordCommandHandler(UserManager<IdentityUser> userManager, IJwtService jwt) : IRequestHandler<ChangePasswordCommand, ChangePasswordResult>
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly IJwtService _jwt = jwt;

    public async Task<ChangePasswordResult> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return ChangePasswordResult.UserNotFound();

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            List<ErrorItem>? errs = [.. result.Errors.Select(e => new ErrorItem(e.Code, e.Description))];
            return ChangePasswordResult.ChangePasswordFailed(errs);
        }

        var refreshTokensRevoked = await _jwt.RevokeUserTokensAsync(user.Id, cancellationToken);

        return ChangePasswordResult.ChangePasswordSuccess(refreshTokensRevoked);
    }
}
