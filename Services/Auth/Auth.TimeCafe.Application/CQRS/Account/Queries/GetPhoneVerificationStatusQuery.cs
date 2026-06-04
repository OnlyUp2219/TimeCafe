namespace Auth.TimeCafe.Application.CQRS.Account.Queries;

public record PhoneVerificationStatusResponse(
    string? PhoneNumber,
    bool PhoneNumberConfirmed,
    bool HasPendingVerification
);

public record GetPhoneVerificationStatusQuery(Guid UserId) : IQuery<PhoneVerificationStatusResponse>;

public class GetPhoneVerificationStatusQueryHandler(
    UserManager<ApplicationUser> userManager,
    ISmsVerificationAttemptTracker attemptTracker
) : IQueryHandler<GetPhoneVerificationStatusQuery, PhoneVerificationStatusResponse>
{
    public async Task<Result<PhoneVerificationStatusResponse>> Handle(GetPhoneVerificationStatusQuery request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            return Result.Fail(new UserNotFoundError(request.UserId));

        var hasPending = !string.IsNullOrEmpty(user.PhoneNumber)
                         && !user.PhoneNumberConfirmed
                         && attemptTracker.HasPendingVerification(request.UserId.ToString(), user.PhoneNumber);

        return Result.Ok(new PhoneVerificationStatusResponse(
            user.PhoneNumber,
            user.PhoneNumberConfirmed,
            hasPending
        ));
    }
}
