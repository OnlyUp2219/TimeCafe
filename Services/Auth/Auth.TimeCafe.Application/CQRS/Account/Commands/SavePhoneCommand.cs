namespace Auth.TimeCafe.Application.CQRS.Account.Commands;

public record SavePhoneCommand(string UserId, string PhoneNumber) : IRequest<SavePhoneResult>;

public record SavePhoneResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    string? PhoneNumber = null
) : ICqrsResultV2
{
    public static SavePhoneResult UserNotFound() =>
        new(false, Code: "UserNotFound", Message: "Пользователь не найден", StatusCode: 401);

    public static SavePhoneResult Saved(string phoneNumber) =>
        new(true, Message: "Номер телефона сохранен", PhoneNumber: phoneNumber);

    public static SavePhoneResult SaveFailed(string phoneNumber) =>
        new(false, Code: "PhoneSaveFailed", Message: "Не удалось сохранить номер телефона", StatusCode: 500,
            PhoneNumber: phoneNumber);
}

public class SavePhoneCommandValidator : AbstractValidator<SavePhoneCommand>
{
    public SavePhoneCommandValidator()
    {
        RuleFor(x => x.UserId).ValidEntityId("Пользователь не найден");

        RuleFor(x => x.PhoneNumber).ValidPhone();
    }
}

public class SavePhoneCommandHandler(
    UserManager<ApplicationUser> userManager
) : IRequestHandler<SavePhoneCommand, SavePhoneResult>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<SavePhoneResult> Handle(SavePhoneCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return SavePhoneResult.UserNotFound();

            var normalizedPhone = NormalizePhone(request.PhoneNumber);
            if (string.Equals(user.PhoneNumber, normalizedPhone, StringComparison.Ordinal) && !user.PhoneNumberConfirmed)
                return SavePhoneResult.Saved(normalizedPhone);

            user.PhoneNumber = normalizedPhone;
            user.PhoneNumberConfirmed = false;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return SavePhoneResult.SaveFailed(normalizedPhone);

            return SavePhoneResult.Saved(normalizedPhone);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(SavePhoneResult.SaveFailed(request.PhoneNumber), ex);
        }
    }

    private static string NormalizePhone(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return phoneNumber;

        var buffer = new char[phoneNumber.Length];
        var idx = 0;
        foreach (var ch in phoneNumber)
        {
            if (char.IsDigit(ch))
                buffer[idx++] = ch;
        }

        if (idx == 0)
            return phoneNumber;
        return "+" + new string(buffer, 0, idx);
    }
}
