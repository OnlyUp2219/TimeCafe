namespace Venue.TimeCafe.Application.CQRS.Loyalty.Queries;

public record GetUserLoyaltyQuery(Guid UserId) : IQuery<UserLoyalty>;

public class GetUserLoyaltyQueryHandler(IUserLoyaltyRepository repository) : IQueryHandler<GetUserLoyaltyQuery, UserLoyalty>
{
    private readonly IUserLoyaltyRepository _repository = repository;

    public async Task<Result<UserLoyalty>> Handle(GetUserLoyaltyQuery request, CancellationToken cancellationToken)
    {
        var loyalty = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (loyalty == null)
            return Result.Ok(new UserLoyalty(request.UserId) { PersonalDiscountPercent = 0 });

        return Result.Ok(loyalty);
    }
}
