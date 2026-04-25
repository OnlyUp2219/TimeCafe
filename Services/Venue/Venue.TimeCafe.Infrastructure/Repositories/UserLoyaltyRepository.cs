namespace Venue.TimeCafe.Infrastructure.Repositories;

public class UserLoyaltyRepository(ApplicationDbContext context) : IUserLoyaltyRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<UserLoyalty?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.UserLoyalties.FirstOrDefaultAsync(x => x.UserId == userId, ct);
    }
}

