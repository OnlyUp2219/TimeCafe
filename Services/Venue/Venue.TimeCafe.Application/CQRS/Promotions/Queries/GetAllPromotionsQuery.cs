namespace Venue.TimeCafe.Application.CQRS.Promotions.Queries;

public record GetAllPromotionsQuery() : IQuery<IEnumerable<Promotion>>;

public class GetAllPromotionsQueryHandler(IUnitOfWork uow) : IQueryHandler<GetAllPromotionsQuery, IEnumerable<Promotion>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<IEnumerable<Promotion>>> Handle(GetAllPromotionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var promotions = await _uow.Promotions.GetAllAsync(cancellationToken);
            return Result.Ok(promotions);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}


