using Auth.TimeCafe.Application.CQRS.Sender.Commands;
using Microsoft.Extensions.Configuration;

namespace Auth.TimeCafe.Application.CQRS.Account.Commands.Phone;

public record PhoneVerificationResponse(string PhoneNumber, string? Token = null, string? Message = null);

public record GeneratePhoneVerificationCommand(Guid UserId, string PhoneNumber, bool Mock = false) : ICommand<PhoneVerificationResponse>;

public class GeneratePhoneVerificationCommandValidator : AbstractValidator<GeneratePhoneVerificationCommand>
{
    public GeneratePhoneVerificationCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");
        RuleFor(x => x.PhoneNumber).ValidPhone();
    }
}

public class GeneratePhoneVerificationCommandHandler(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    ISender sender,
    ISmsVerificationAttemptTracker attemptTracker
) : ICommandHandler<GeneratePhoneVerificationCommand, PhoneVerificationResponse>
{
    public async Task<Result<PhoneVerificationResponse>> Handle(GeneratePhoneVerificationCommand request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            return Result.Fail(new PhoneUserNotFoundError(request.UserId));

        var token = await userManager.GenerateChangePhoneNumberTokenAsync(user, request.PhoneNumber);

        if (request.Mock)
        {
            attemptTracker.ResetAttempts(request.UserId.ToString(), request.PhoneNumber);
            attemptTracker.RecordCodeSent(request.UserId.ToString(), request.PhoneNumber);
            return Result.Ok(new PhoneVerificationResponse(request.PhoneNumber, token, "Mock SMS сгенерировано"));
        }

        string accountSid = configuration["Twilio:AccountSid"] ?? string.Empty;
        string authToken = configuration["Twilio:AuthToken"] ?? string.Empty;
        string twilioPhoneNumber = configuration["Twilio:TwilioPhoneNumber"] ?? string.Empty;

        var sendSmsCommand = new SendSmsCommand(accountSid, authToken, twilioPhoneNumber, request.PhoneNumber, token);
        var sendResult = await sender.Send(sendSmsCommand, cancellationToken);

        if (sendResult != null)
        {
            attemptTracker.ResetAttempts(request.UserId.ToString(), request.PhoneNumber);
            attemptTracker.RecordCodeSent(request.UserId.ToString(), request.PhoneNumber);
            return Result.Ok(new PhoneVerificationResponse(request.PhoneNumber, Message: "SMS отправлено"));
        }

        return Result.Fail(new PhoneVerificationSendFailedError(request.PhoneNumber));
    }
}
