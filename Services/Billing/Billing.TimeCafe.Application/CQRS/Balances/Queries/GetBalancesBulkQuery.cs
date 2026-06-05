namespace Billing.TimeCafe.Application.CQRS.Balances.Queries;

public sealed record GetBalancesBulkQuery(IEnumerable<Guid> UserIds) : IQuery<IDictionary<Guid, decimal>>;

public sealed class GetBalancesBulkQueryHandler(IUnitOfWork uow) : IQueryHandler<GetBalancesBulkQuery, IDictionary<Guid, decimal>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<IDictionary<Guid, decimal>>> Handle(GetBalancesBulkQuery request, CancellationToken cancellationToken = default)
    {
        var balances = await _uow.Balances.GetByUserIdsAsync(request.UserIds, cancellationToken);
        var dictionary = balances.ToDictionary(b => b.UserId, b => b.CurrentBalance);
        return Result.Ok((IDictionary<Guid, decimal>)dictionary);
    }
}
