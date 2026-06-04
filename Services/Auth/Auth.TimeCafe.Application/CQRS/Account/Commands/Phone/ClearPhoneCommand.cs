namespace Auth.TimeCafe.Application.CQRS.Account.Commands.Phone;

public record ClearPhoneCommand(Guid UserId) : ICommand;

public class ClearPhoneCommandValidator : AbstractValidator<ClearPhoneCommand>
{
    public ClearPhoneCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");
    }
}

public class ClearPhoneCommandHandler(
    UserManager<ApplicationUser> userManager,
    MediatR.IPublisher publisher
) : ICommandHandler<ClearPhoneCommand>
{
    public async Task<Result> Handle(ClearPhoneCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return Result.Fail(new UserNotFoundError(request.UserId));

            if (string.IsNullOrWhiteSpace(user.PhoneNumber) && !user.PhoneNumberConfirmed)
                return Result.Ok();

            user.PhoneNumber = null;
            user.PhoneNumberConfirmed = false;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return Result.Fail(new PhoneClearFailedError(request.UserId));

            await publisher.Publish(new Events.UserChangedEvent(user.Id), cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
