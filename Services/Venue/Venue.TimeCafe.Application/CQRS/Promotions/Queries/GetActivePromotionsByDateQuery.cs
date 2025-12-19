namespace Venue.TimeCafe.Application.CQRS.Promotions.Queries;

public record GetActivePromotionsByDateQuery(DateTimeOffset Date) : IRequest<GetActivePromotionsByDateResult>;

public record GetActivePromotionsByDateResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<Promotion>? Promotions = null) : ICqrsResultV2
{
    public static GetActivePromotionsByDateResult GetFailed() =>
        new(false, Code: "GetActivePromotionsByDateFailed", Message: "Не удалось получить активные акции на дату", StatusCode: 500);

    public static GetActivePromotionsByDateResult GetSuccess(IEnumerable<Promotion> promotions) =>
        new(true, Promotions: promotions);
}

public class GetActivePromotionsByDateQueryHandler(IPromotionRepository repository) : IRequestHandler<GetActivePromotionsByDateQuery, GetActivePromotionsByDateResult>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<GetActivePromotionsByDateResult> Handle(GetActivePromotionsByDateQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var promotions = await _repository.GetActiveByDateAsync(request.Date);
            return GetActivePromotionsByDateResult.GetSuccess(promotions);
        }
        catch (Exception)
        {
            return GetActivePromotionsByDateResult.GetFailed();
        }
    }
}

public class GetActivePromotionsByDateQueryValidator : AbstractValidator<GetActivePromotionsByDateQuery>
{
    public GetActivePromotionsByDateQueryValidator()
    {
    }
}
