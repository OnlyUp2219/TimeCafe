namespace Venue.TimeCafe.Application.CQRS.Promotions.Queries;

public record GetActivePromotionsByDateQuery(DateTimeOffset Date) : IQuery<IEnumerable<Promotion>>;

public class GetActivePromotionsByDateQueryHandler(IPromotionRepository repository) : IQueryHandler<GetActivePromotionsByDateQuery, IEnumerable<Promotion>>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<Result<IEnumerable<Promotion>>> Handle(GetActivePromotionsByDateQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var promotions = await _repository.GetActiveByDateAsync(request.Date);
            return Result.Ok(promotions);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

public class GetActivePromotionsByDateQueryValidator : AbstractValidator<GetActivePromotionsByDateQuery>
{
    public GetActivePromotionsByDateQueryValidator()
    {
    }
}

