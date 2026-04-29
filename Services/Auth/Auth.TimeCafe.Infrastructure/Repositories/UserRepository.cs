using System.Diagnostics;

namespace Auth.TimeCafe.Infrastructure.Repositories;

public class UserRepository(
    UserManager<ApplicationUser> userManager,
    HybridCache cache,
    ApplicationDbContext context,
    ILogger<UserRepository> logger) : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly HybridCache _cache = cache;
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<UserRepository> _logger = logger;

    public async Task<(List<ApplicationUser> Users, int TotalCount)> GetUsersPageAsync(
        int page, int pageSize, string? search, string? status, CancellationToken ct)
    {
        var cacheKey = $"users_page_{page}_{pageSize}_{search ?? "null"}_{status ?? "null"}";

        var stopwatch = Stopwatch.StartNew();
        var result = await _cache.GetOrCreateAsync(
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

        stopwatch.Stop();
        if (stopwatch.ElapsedMilliseconds > 1000)
        {
            _logger.LogWarning(
                "Slow GetUsersPageAsync call: {ElapsedMs}ms; page={Page}; pageSize={PageSize}; search={Search}; status={Status}",
                stopwatch.ElapsedMilliseconds,
                page,
                pageSize,
                search,
                status);
        }

        return result;
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

    public async Task<Dictionary<Guid, List<string>>> GetUsersRolesBatchAsync(IEnumerable<Guid> userIds, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        var userIdList = userIds.ToList();
        if (userIdList.Count == 0)
            return new Dictionary<Guid, List<string>>();

        var roleAssignments = await _context.UserRoles
            .Where(ur => userIdList.Contains(ur.UserId))
            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, RoleName = r.Name! })
            .ToListAsync(ct);

        var result = roleAssignments
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToList());

        stopwatch.Stop();
        if (stopwatch.ElapsedMilliseconds > 500)
        {
            _logger.LogWarning(
                "Slow GetUsersRolesBatchAsync call: {ElapsedMs}ms; userCount={UserCount}",
                stopwatch.ElapsedMilliseconds,
                userIdList.Count);
        }

        return result;
    }

    public async Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken ct)
    {
        return await _userManager.FindByIdAsync(userId.ToString());
    }
}