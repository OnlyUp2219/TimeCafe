namespace Venue.TimeCafe.Application.CQRS.Promotions.Queries;

public record GetPromotionByIdQuery(Guid PromotionId) : IQuery<Promotion>;

public class GetPromotionByIdQueryValidator : AbstractValidator<GetPromotionByIdQuery>
{
    public GetPromotionByIdQueryValidator()
    {
        RuleFor(x => x.PromotionId).ValidGuidEntityId("Акция не найдена");
    }
}

public class GetPromotionByIdQueryHandler(IPromotionRepository repository) : IQueryHandler<GetPromotionByIdQuery, Promotion>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<Result<Promotion>> Handle(GetPromotionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var promotion = await _repository.GetByIdAsync(request.PromotionId);

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

