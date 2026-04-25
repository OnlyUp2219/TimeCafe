namespace Venue.TimeCafe.Application.CQRS.Promotions.Queries;

public record GetActivePromotionsQuery() : IQuery<IEnumerable<Promotion>>;

public class GetActivePromotionsQueryHandler(IPromotionRepository repository) : IQueryHandler<GetActivePromotionsQuery, IEnumerable<Promotion>>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<Result<IEnumerable<Promotion>>> Handle(GetActivePromotionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var promotions = await _repository.GetActiveAsync();
            return Result.Ok(promotions);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

public class GetActivePromotionsQueryValidator : AbstractValidator<GetActivePromotionsQuery>
{
    public GetActivePromotionsQueryValidator()
    {
    }
}

