namespace Auth.TimeCafe.Application.CQRS.Admin.Query;

public sealed record GetUserByIdQuery(Guid UserId) : IRequest<Result<AdminUserResponse>>;


public sealed class GetUserByIdQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUserByIdQuery, Result<AdminUserResponse>>
{
    public async Task<Result<AdminUserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Fail(new Error("Пользователь не найден").WithMetadata("StatusCode", 404));

        var roles = await userRepository.GetUserRolesAsync(request.UserId, cancellationToken);
        var status = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.Now ? "inactive" : "active";

        return Result.Ok(new AdminUserResponse
        {
            Id = user.Id,
            Email = user.Email!,
            Name = user.UserName,
            Role = string.Join(", ", roles),
            Status = status,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
        });
    }
}
