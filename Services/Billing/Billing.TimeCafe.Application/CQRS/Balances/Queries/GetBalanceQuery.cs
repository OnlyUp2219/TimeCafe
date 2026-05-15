namespace Billing.TimeCafe.Application.CQRS.Balances.Queries;

public sealed record GetBalanceQuery(Guid UserId) : IQuery<GetBalanceResponse>;

public sealed record GetBalanceResponse(
    Guid UserId,
    decimal CurrentBalance,
    decimal TotalDeposited,
    decimal TotalSpent,
    decimal Debt,
    DateTimeOffset LastUpdated);

public sealed class GetBalanceQueryHandler(IUnitOfWork uow) : IQueryHandler<GetBalanceQuery, GetBalanceResponse>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<GetBalanceResponse>> Handle(GetBalanceQuery request, CancellationToken cancellationToken = default)
    {
        var balance = await _uow.Balances.GetByIdAsync(request.UserId, cancellationToken);

        if (balance == null)
        {
            balance = Balance.Create(request.UserId);
            await _uow.Balances.CreateAsync(balance, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok(new GetBalanceResponse(
            balance.UserId,
            balance.CurrentBalance,
            balance.TotalDeposited,
            balance.TotalSpent,
            balance.Debt,
            balance.LastUpdated));
    }
}
