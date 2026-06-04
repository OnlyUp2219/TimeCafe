namespace Auth.TimeCafe.Application.CQRS.Account.Commands.Phone;

public record SavePhoneCommand(Guid UserId, string PhoneNumber) : ICommand<string>;

public class SavePhoneCommandValidator : AbstractValidator<SavePhoneCommand>
{
    public SavePhoneCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");
        RuleFor(x => x.PhoneNumber).ValidPhone();
    }
}

public class SavePhoneCommandHandler(
    UserManager<ApplicationUser> userManager,
    MediatR.IPublisher publisher
) : ICommandHandler<SavePhoneCommand, string>
{
    public async Task<Result<string>> Handle(SavePhoneCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return Result.Fail(new UserNotFoundError(request.UserId));

            var normalizedPhone = NormalizePhone(request.PhoneNumber);
            if (string.Equals(user.PhoneNumber, normalizedPhone, StringComparison.Ordinal) && !user.PhoneNumberConfirmed)
                return Result.Ok(normalizedPhone);

            user.PhoneNumber = normalizedPhone;
            user.PhoneNumberConfirmed = false;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return Result.Fail(new PhoneSaveFailedError(normalizedPhone));

            await publisher.Publish(new Events.UserChangedEvent(user.Id), cancellationToken);

            return Result.Ok(normalizedPhone);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
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
