namespace Venue.TimeCafe.Application.CQRS.Promotions.Queries;

public record GetAllPromotionsQuery() : IRequest<GetAllPromotionsResult>;

public record GetAllPromotionsResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<Promotion>? Promotions = null) : ICqrsResultV2
{
    public static GetAllPromotionsResult GetFailed() =>
        new(false, Code: "GetPromotionsFailed", Message: "Не удалось получить акции", StatusCode: 500);

    public static GetAllPromotionsResult GetSuccess(IEnumerable<Promotion> promotions) =>
        new(true, Promotions: promotions);
}

public class GetAllPromotionsQueryHandler(IPromotionRepository repository) : IRequestHandler<GetAllPromotionsQuery, GetAllPromotionsResult>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<GetAllPromotionsResult> Handle(GetAllPromotionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var promotions = await _repository.GetAllAsync();
            return GetAllPromotionsResult.GetSuccess(promotions);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(GetAllPromotionsResult.GetFailed(), ex);
        }
    }
}

public class GetAllPromotionsQueryValidator : AbstractValidator<GetAllPromotionsQuery>
{
    public GetAllPromotionsQueryValidator()
    {
    }
}
