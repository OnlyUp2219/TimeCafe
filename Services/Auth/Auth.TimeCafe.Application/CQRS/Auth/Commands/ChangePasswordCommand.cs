namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record ChangePasswordResult(bool Success, string? Message = null, List<string>? Errors = null, ETypeError? TypeError = null, int? RefreshTokensRevoked = null);

public record ChangePasswordCommand(string UserId, string CurrentPassword, string NewPassword) : IRequest<ChangePasswordResult>;

public class ChangePasswordCommandHandler(UserManager<IdentityUser> userManager, IJwtService jwt) : IRequestHandler<ChangePasswordCommand, ChangePasswordResult>
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly IJwtService _jwt = jwt;

    public async Task<ChangePasswordResult> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            return new ChangePasswordResult(false, Message: "Требуется заполнить пароли", Errors: [ "EmptyFields" ]);

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return new ChangePasswordResult(false, TypeError: ETypeError.Unauthorized);

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errs = result.Errors.Select(e => $"{e.Code}: {e.Description}").ToList();
            return new ChangePasswordResult(false, "Не удалось сменить пароль", errs, ETypeError.IdentityError);

        }

        var refreshTokensRevoked = await _jwt.RevokeUserTokensAsync(user.Id);

        return new ChangePasswordResult( true ,Message: "Пароль изменён", RefreshTokensRevoked: refreshTokensRevoked);
    }
}
