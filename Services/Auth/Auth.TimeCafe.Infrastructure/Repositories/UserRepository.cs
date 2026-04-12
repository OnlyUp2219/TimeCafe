namespace Auth.TimeCafe.Infrastructure.Repositories;

public class UserRepository(UserManager<ApplicationUser> userManager, HybridCache cache) : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly HybridCache _cache = cache;

    public async Task<(List<ApplicationUser> Users, int TotalCount)> GetUsersPageAsync(
        int page, int pageSize, string? search, string? status, CancellationToken ct)
    {
        var cacheKey = $"users_page_{page}_{pageSize}_{search ?? "null"}_{status ?? "null"}";

        return await _cache.GetOrCreateAsync(
            cacheKey,
            async token =>
            {
                var query = _userManager.Users.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(u => u.Email!.Contains(search) || u.UserName!.Contains(search));
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    if (status == "active")
                    {
                        query = query.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.Now);
                    }
                    else if (status == "inactive")
                    {
                        query = query.Where(u => u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.Now);
                    }
                }

                var totalCount = await query.CountAsync(token);
                var users = await query
                    .OrderBy(u => u.Email)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(token);

                return (users, totalCount);
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: ["users"],
            cancellationToken: ct);
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId, CancellationToken ct)
    {
        var cacheKey = $"user_roles_{userId}";

        return await _cache.GetOrCreateAsync(
            cacheKey,
            async token =>
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                return user != null ? (await _userManager.GetRolesAsync(user)).ToList() : new List<string>();
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(10) },
            tags: ["user_roles", $"user_{userId}"],
            cancellationToken: ct);
    }
}