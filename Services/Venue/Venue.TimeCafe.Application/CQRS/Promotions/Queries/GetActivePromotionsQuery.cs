namespace Venue.TimeCafe.Application.CQRS.Promotions.Queries;

public record GetActivePromotionsQuery() : IQuery<IEnumerable<Promotion>>;

public class GetActivePromotionsQueryHandler(IUnitOfWork uow) : IQueryHandler<GetActivePromotionsQuery, IEnumerable<Promotion>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<IEnumerable<Promotion>>> Handle(GetActivePromotionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var promotions = await _uow.Promotions.GetActiveAsync(cancellationToken);
            return Result.Ok(promotions);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

