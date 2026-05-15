namespace Billing.TimeCafe.Application.CQRS.Balances.Queries;

public sealed record GetUserDebtQuery(Guid UserId) : IQuery<GetUserDebtResponse>;

public sealed record GetUserDebtResponse(decimal Debt);

public sealed class GetUserDebtQueryHandler(IUnitOfWork uow) : IQueryHandler<GetUserDebtQuery, GetUserDebtResponse>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<GetUserDebtResponse>> Handle(GetUserDebtQuery request, CancellationToken cancellationToken = default)
    {
        var balance = await _uow.Balances.GetByIdAsync(request.UserId, cancellationToken);
        if (balance == null)
        {
            balance = Balance.Create(request.UserId);
            await _uow.Balances.CreateAsync(balance, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok(new GetUserDebtResponse(balance.Debt));
    }
}
