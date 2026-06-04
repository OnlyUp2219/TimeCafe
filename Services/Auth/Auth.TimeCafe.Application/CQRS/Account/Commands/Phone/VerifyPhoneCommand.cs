namespace Auth.TimeCafe.Application.CQRS.Account.Commands.Phone;

public record VerifyPhoneResponse(
    string PhoneNumber,
    int RemainingAttempts,
    bool RequiresCaptcha,
    string? Message = null
);

public record VerifyPhoneCommand(Guid UserId, string PhoneNumber, string Code, string? CaptchaToken, bool Mock = false) : ICommand<VerifyPhoneResponse>;

public class VerifyPhoneCommandValidator : AbstractValidator<VerifyPhoneCommand>
{
    public VerifyPhoneCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");
        RuleFor(x => x.PhoneNumber).ValidPhone();
    }
}

public class VerifyPhoneCommandHandler(
    UserManager<ApplicationUser> userManager,
    ISmsVerificationAttemptTracker attemptTracker,
    ICaptchaValidator captchaValidator,
    MediatR.IPublisher publisher
) : ICommandHandler<VerifyPhoneCommand, VerifyPhoneResponse>
{
    public async Task<Result<VerifyPhoneResponse>> Handle(VerifyPhoneCommand request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            var error = new PhoneUserNotFoundError(request.UserId);
            error.Metadata.Add("RemainingAttempts", 0);
            error.Metadata.Add("RequiresCaptcha", false);
            error.Metadata.Add("PhoneNumber", request.PhoneNumber);
            return Result.Fail(error);
        }

        if (!attemptTracker.CanVerifyCode(request.UserId.ToString(), request.PhoneNumber))
        {
            var error = new PhoneVerificationTooManyAttemptsError(request.PhoneNumber);
            error.Metadata.Add("RemainingAttempts", 0);
            error.Metadata.Add("RequiresCaptcha", true);
            return Result.Fail(error);
        }

        var remaining = attemptTracker.GetRemainingAttempts(request.UserId.ToString(), request.PhoneNumber);
        if (remaining == 3)
        {
            if (string.IsNullOrEmpty(request.CaptchaToken))
            {
                var error = new PhoneVerificationCaptchaRequiredError(request.PhoneNumber);
                error.Metadata.Add("RemainingAttempts", remaining);
                error.Metadata.Add("RequiresCaptcha", true);
                return Result.Fail(error);
            }

            if (!await captchaValidator.ValidateAsync(request.CaptchaToken))
            {
                var error = new PhoneVerificationCaptchaInvalidError(request.PhoneNumber);
                error.Metadata.Add("RemainingAttempts", remaining);
                error.Metadata.Add("RequiresCaptcha", true);
                return Result.Fail(error);
            }
        }

        var identityResult = await userManager.ChangePhoneNumberAsync(user, request.PhoneNumber, request.Code);
        if (identityResult.Succeeded)
        {
            attemptTracker.ResetAttempts(request.UserId.ToString(), request.PhoneNumber);
            attemptTracker.ClearPendingVerification(request.UserId.ToString(), request.PhoneNumber);
            var attempts = attemptTracker.GetRemainingAttempts(request.UserId.ToString(), request.PhoneNumber);

            await publisher.Publish(new Events.UserChangedEvent(user.Id), cancellationToken);

            return Result.Ok(new VerifyPhoneResponse(
                request.PhoneNumber,
                attempts,
                false,
                request.Mock ? "Номер телефона успешно подтвержден (mock)" : "Номер телефона успешно подтвержден"
            ));
        }

        attemptTracker.RecordFailedAttempt(request.UserId.ToString(), request.PhoneNumber);
        remaining = attemptTracker.GetRemainingAttempts(request.UserId.ToString(), request.PhoneNumber);

        if (remaining <= 0)
        {
            var error = new PhoneVerificationTooManyAttemptsError(request.PhoneNumber);
            error.Metadata.Add("RemainingAttempts", remaining);
            error.Metadata.Add("RequiresCaptcha", true);
            return Result.Fail(error);
        }

        var invalidCodeError = new PhoneVerificationInvalidCodeError(request.PhoneNumber);
        invalidCodeError.Metadata.Add("RemainingAttempts", remaining);
        invalidCodeError.Metadata.Add("RequiresCaptcha", remaining == 3);
        return Result.Fail(invalidCodeError);
    }
}
