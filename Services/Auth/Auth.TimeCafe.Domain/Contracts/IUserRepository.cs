namespace Auth.TimeCafe.Domain.Contracts;

public interface IUserRepository
{
    Task<(List<ApplicationUser> Users, int TotalCount)> GetUsersPageAsync(
        int page, int pageSize, string? search = null, string? status = null, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, List<string>>> GetUsersRolesBatchAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}