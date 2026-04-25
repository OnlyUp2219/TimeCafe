namespace Venue.TimeCafe.Application.CQRS.Promotions.Queries;

public record GetAllPromotionsQuery() : IQuery<IEnumerable<Promotion>>;

public class GetAllPromotionsQueryHandler(IPromotionRepository repository) : IQueryHandler<GetAllPromotionsQuery, IEnumerable<Promotion>>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<Result<IEnumerable<Promotion>>> Handle(GetAllPromotionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var promotions = await _repository.GetAllAsync();
            return Result.Ok(promotions);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

public class GetAllPromotionsQueryValidator : AbstractValidator<GetAllPromotionsQuery>
{
    public GetAllPromotionsQueryValidator()
    {
    }
}

