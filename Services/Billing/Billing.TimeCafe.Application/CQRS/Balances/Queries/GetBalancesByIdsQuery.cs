namespace Billing.TimeCafe.Application.CQRS.Balances.Queries;

public sealed record GetBalancesByIdsQuery(IEnumerable<Guid> UserIds) : IQuery<GetBalancesByIdsResponse>;

public sealed record GetBalancesByIdsResponse(List<GetBalanceResponse> Balances);

public sealed class GetBalancesByIdsQueryHandler(IUnitOfWork uow) : IQueryHandler<GetBalancesByIdsQuery, GetBalancesByIdsResponse>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<GetBalancesByIdsResponse>> Handle(GetBalancesByIdsQuery request, CancellationToken cancellationToken = default)
    {
        var balances = await _uow.Balances.GetByUserIdsAsync(request.UserIds, cancellationToken);
        
        var dtos = balances.Select(b => new GetBalanceResponse(
            b.UserId,
            b.CurrentBalance,
            b.TotalDeposited,
            b.TotalSpent,
            b.Debt,
            b.LastUpdated)).ToList();

        return Result.Ok(new GetBalancesByIdsResponse(dtos));
    }
}
