namespace Auth.TimeCafe.Domain.Contracts;

public interface IUserRepository
{
    Task<(List<ApplicationUser> Users, int TotalCount)> GetUsersPageAsync(
        int page, int pageSize, string? search = null, string? status = null, CancellationToken ct = default);
    Task<List<string>> GetUserRolesAsync(Guid userId, CancellationToken ct = default);
}