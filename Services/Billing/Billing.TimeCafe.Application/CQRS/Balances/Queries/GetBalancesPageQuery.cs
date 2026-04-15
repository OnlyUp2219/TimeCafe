namespace Billing.TimeCafe.Application.CQRS.Balances.Queries;

public record GetBalancesPageQuery(int Page, int PageSize) : IRequest<GetBalancesPageResult>;

public record GetBalancesPageResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    List<BalanceDto>? Balances = null,
    int? TotalCount = null,
    int? TotalPages = null) : ICqrsResult
{
    public static GetBalancesPageResult GetSuccess(List<BalanceDto> balances, int totalCount, int pageSize) =>
        new(true, Message: "Балансы получены",
            Balances: balances,
            TotalCount: totalCount,
            TotalPages: (totalCount + pageSize - 1) / pageSize);
}

public class GetBalancesPageQueryValidator : AbstractValidator<GetBalancesPageQuery>
{
    public GetBalancesPageQueryValidator()
    {
        RuleFor(x => x.Page).ValidPageNumber();
        RuleFor(x => x.PageSize).ValidPageSize();
    }
}

public class GetBalancesPageQueryHandler(IBalanceRepository repository)
    : IRequestHandler<GetBalancesPageQuery, GetBalancesPageResult>
{
    public async Task<GetBalancesPageResult> Handle(GetBalancesPageQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetPageAsync(request.Page, request.PageSize, cancellationToken);

        var dtos = items.ConvertAll(b => new BalanceDto(
            b.UserId,
            b.CurrentBalance,
            b.TotalDeposited,
            b.TotalSpent,
            b.Debt,
            b.LastUpdated,
            b.CreatedAt));

        return GetBalancesPageResult.GetSuccess(dtos, totalCount, request.PageSize);
    }
}
