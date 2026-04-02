namespace Auth.TimeCafe.Application.CQRS.Account.Queries;

public record GetPhoneVerificationStatusQuery(string UserId) : IRequest<GetPhoneVerificationStatusResult>;

public record GetPhoneVerificationStatusResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    string? PhoneNumber = null,
    bool PhoneNumberConfirmed = false,
    bool HasPendingVerification = false
) : ICqrsResultV2
{
    public static GetPhoneVerificationStatusResult UserNotFound() =>
        new(false, Code: "UserNotFound", Message: "Пользователь не найден", StatusCode: 401);

    public static GetPhoneVerificationStatusResult Ok(string? phoneNumber, bool confirmed, bool pending) =>
        new(true, PhoneNumber: phoneNumber, PhoneNumberConfirmed: confirmed, HasPendingVerification: pending);
}

public class GetPhoneVerificationStatusQueryValidator : AbstractValidator<GetPhoneVerificationStatusQuery>
{
    public GetPhoneVerificationStatusQueryValidator()
    {
        RuleFor(x => x.UserId).ValidEntityId("Пользователь не найден");
    }
}

public class GetPhoneVerificationStatusQueryHandler(
    UserManager<ApplicationUser> userManager,
    ISmsVerificationAttemptTracker attemptTracker
) : IRequestHandler<GetPhoneVerificationStatusQuery, GetPhoneVerificationStatusResult>
{
    public async Task<GetPhoneVerificationStatusResult> Handle(GetPhoneVerificationStatusQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return GetPhoneVerificationStatusResult.UserNotFound();

        var hasPending = !string.IsNullOrEmpty(user.PhoneNumber)
                         && !user.PhoneNumberConfirmed
                         && attemptTracker.HasPendingVerification(request.UserId, user.PhoneNumber);

        return GetPhoneVerificationStatusResult.Ok(
            user.PhoneNumber,
            user.PhoneNumberConfirmed,
            hasPending);
    }
}
