namespace Venue.TimeCafe.Application.CQRS.Promotions.Queries;

public record GetActivePromotionsByDateQuery(DateTimeOffset Date) : IQuery<IEnumerable<Promotion>>;

public class GetActivePromotionsByDateQueryHandler(IUnitOfWork uow) : IQueryHandler<GetActivePromotionsByDateQuery, IEnumerable<Promotion>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<IEnumerable<Promotion>>> Handle(GetActivePromotionsByDateQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var promotions = await _uow.Promotions.GetActiveByDateAsync(request.Date, cancellationToken);
            return Result.Ok(promotions);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
