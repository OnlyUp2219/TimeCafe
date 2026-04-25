namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface IUserLoyaltyRepository
{
    Task<UserLoyalty?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}

