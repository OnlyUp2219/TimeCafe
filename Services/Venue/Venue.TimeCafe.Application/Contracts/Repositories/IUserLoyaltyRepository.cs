namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface IUserLoyaltyRepository : IRepository<UserLoyalty, Guid>
{
    Task<UserLoyalty?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

