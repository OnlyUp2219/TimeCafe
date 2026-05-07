namespace Venue.TimeCafe.Application.CQRS.Promotions.Queries;

public record GetPromotionByIdQuery(Guid PromotionId) : IQuery<Promotion>;

public class GetPromotionByIdQueryHandler(IUnitOfWork uow) : IQueryHandler<GetPromotionByIdQuery, Promotion>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<Promotion>> Handle(GetPromotionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var promotion = await _uow.Promotions.GetByIdAsync(request.PromotionId, cancellationToken);

            if (promotion == null)
                return Result.Fail(new PromotionNotFoundError());

            return Result.Ok(promotion);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

