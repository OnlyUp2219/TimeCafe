namespace Billing.TimeCafe.Application.CQRS.Balances.Queries;

public sealed record GetBalancesPageQuery(int Page, int PageSize) : IQuery<GetBalancesPageResponse>;

public sealed record GetBalancesPageResponse(List<BalanceDto> Balances, int TotalCount, int TotalPages);

public sealed class GetBalancesPageQueryHandler(IUnitOfWork uow)
    : IQueryHandler<GetBalancesPageQuery, GetBalancesPageResponse>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<GetBalancesPageResponse>> Handle(GetBalancesPageQuery request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _uow.Balances.GetPageAsync(request.Page, request.PageSize, cancellationToken);

        var dtos = items.ConvertAll(b => new BalanceDto(
            b.UserId,
            b.CurrentBalance,
            b.TotalDeposited,
            b.TotalSpent,
            b.Debt,
            b.LastUpdated,
            b.CreatedAt));

        var totalPages = (totalCount + request.PageSize - 1) / request.PageSize;

        return Result.Ok(new GetBalancesPageResponse(dtos, totalCount, totalPages));
    }
}
