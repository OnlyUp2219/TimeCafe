namespace Billing.TimeCafe.Application.CQRS.Balances.Queries;

public record GetBalancesByIdsQuery(IEnumerable<Guid> UserIds) : IQuery<List<Balance>>;

public class GetBalancesByIdsQueryHandler(IBalanceRepository repository) : IQueryHandler<GetBalancesByIdsQuery, List<Balance>>
{
    private readonly IBalanceRepository _repository = repository;

    public async Task<Result<List<Balance>>> Handle(GetBalancesByIdsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var balances = await _repository.GetByUserIdsAsync(request.UserIds, cancellationToken);
            return Result.Ok(balances);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Не удалось получить балансы").CausedBy(ex));
        }
    }
}
