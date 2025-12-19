using Venue.TimeCafe.Application.Contracts.Repositories;

namespace Venue.TimeCafe.Application.CQRS.Promotions.Queries;

public record GetActivePromotionsQuery() : IRequest<GetActivePromotionsResult>;

public record GetActivePromotionsResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<Promotion>? Promotions = null) : ICqrsResultV2
{
    public static GetActivePromotionsResult GetFailed() =>
        new(false, Code: "GetActivePromotionsFailed", Message: "Не удалось получить активные акции", StatusCode: 500);

    public static GetActivePromotionsResult GetSuccess(IEnumerable<Promotion> promotions) =>
        new(true, Promotions: promotions);
}

public class GetActivePromotionsQueryHandler(IPromotionRepository repository) : IRequestHandler<GetActivePromotionsQuery, GetActivePromotionsResult>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<GetActivePromotionsResult> Handle(GetActivePromotionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var promotions = await _repository.GetActiveAsync();
            return GetActivePromotionsResult.GetSuccess(promotions);
        }
        catch (Exception)
        {
            return GetActivePromotionsResult.GetFailed();
        }
    }
}

public class GetActivePromotionsQueryValidator : AbstractValidator<GetActivePromotionsQuery>
{
    public GetActivePromotionsQueryValidator()
    {
    }
}
