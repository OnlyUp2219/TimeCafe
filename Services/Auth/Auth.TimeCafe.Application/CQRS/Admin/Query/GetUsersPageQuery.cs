using BuildingBlocks.Contracts.CQRS;

namespace Auth.TimeCafe.Application.CQRS.Admin.Query;

public sealed record GetUsersPageQuery(int Page, int Size, string? Search, string? Status) : IQuery<PagedResponse<AdminUserResponse>>;


public sealed class GetUsersPageQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUsersPageQuery, PagedResponse<AdminUserResponse>>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<Result<PagedResponse<AdminUserResponse>>> Handle(GetUsersPageQuery request, CancellationToken cancellationToken = default)
    {
        var (users, totalCount) = await _userRepository.GetUsersPageAsync(
            request.Page, request.Size, request.Search, request.Status, cancellationToken);

        var rolesMap = await _userRepository.GetUsersRolesBatchAsync(
            users.Select(u => u.Id), cancellationToken);

        var userResponses = users.Select(user =>
        {
            var role = rolesMap.TryGetValue(user.Id, out var roles) ? string.Join(", ", roles) : string.Empty;
            var status = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.Now ? "inactive" : "active";

            return new AdminUserResponse
            {
                Id = user.Id,
                Email = user.Email!,
                Name = user.UserName,
                Role = role,
                Status = status,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed
            };
        }).ToList();

        var totalPages = (totalCount + request.Size - 1) / request.Size;

        return Result.Ok(new PagedResponse<AdminUserResponse>(
            userResponses, 
            new PageMetadata(request.Page, request.Size, totalCount, totalPages)));
    }
}