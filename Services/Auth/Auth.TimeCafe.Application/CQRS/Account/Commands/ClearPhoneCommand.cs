namespace Auth.TimeCafe.Application.CQRS.Account.Commands;

public record ClearPhoneCommand(string UserId) : IRequest<ClearPhoneResult>;

public record ClearPhoneResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null
) : ICqrsResultV2
{
    public static ClearPhoneResult UserNotFound() =>
        new(false, Code: "UserNotFound", Message: "Пользователь не найден", StatusCode: 401);

    public static ClearPhoneResult Cleared() =>
        new(true, Message: "Номер телефона удален");

    public static ClearPhoneResult ClearFailed() =>
        new(false, Code: "PhoneClearFailed", Message: "Не удалось удалить номер телефона", StatusCode: 500);
}

public class ClearPhoneCommandValidator : AbstractValidator<ClearPhoneCommand>
{
    public ClearPhoneCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден")
            .NotNull().WithMessage("Пользователь не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Пользователь не найден");
    }
}

public class ClearPhoneCommandHandler(
    UserManager<ApplicationUser> userManager
) : IRequestHandler<ClearPhoneCommand, ClearPhoneResult>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<ClearPhoneResult> Handle(ClearPhoneCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return ClearPhoneResult.UserNotFound();

            if (string.IsNullOrWhiteSpace(user.PhoneNumber) && user.PhoneNumberConfirmed == false)
                return ClearPhoneResult.Cleared();

            user.PhoneNumber = null;
            user.PhoneNumberConfirmed = false;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return ClearPhoneResult.ClearFailed();

            return ClearPhoneResult.Cleared();
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(ClearPhoneResult.ClearFailed(), ex);
        }
    }
}
