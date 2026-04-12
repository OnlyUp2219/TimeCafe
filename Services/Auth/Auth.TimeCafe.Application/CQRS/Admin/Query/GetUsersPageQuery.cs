namespace Auth.TimeCafe.Application.CQRS.Admin.Query;

public sealed record GetUsersPageQuery(int Page, int Size, string? Search, string? Status) : IQuery<(List<AdminUserResponse> Users, int TotalCount)>;

public sealed class GetUsersPageQueryValidator : AbstractValidator<GetUsersPageQuery>
{
    public GetUsersPageQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.Size).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(x => x.Status).Must(status => status == null || status == "active" || status == "inactive").WithMessage("Status must be 'active' or 'inactive'");
    }
}

public sealed class GetUsersPageQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUsersPageQuery, (List<AdminUserResponse> Users, int TotalCount)>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<Result<(List<AdminUserResponse> Users, int TotalCount)>> Handle(GetUsersPageQuery request, CancellationToken cancellationToken)
    {
        var (users, totalCount) = await _userRepository.GetUsersPageAsync(
            request.Page, request.Size, request.Search, request.Status, cancellationToken);

        var userResponses = new List<AdminUserResponse>();

        foreach (var user in users)
        {
            var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
            var role = roles.Count > 0 ? string.Join(", ", roles) : string.Empty;

            var status = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.Now ? "inactive" : "active";

            userResponses.Add(new AdminUserResponse
            {
                Id = user.Id,
                Email = user.Email!,
                Name = user.UserName,
                Role = role,
                Status = status
            });
        }

        return Result.Ok((userResponses, totalCount));
    }
}