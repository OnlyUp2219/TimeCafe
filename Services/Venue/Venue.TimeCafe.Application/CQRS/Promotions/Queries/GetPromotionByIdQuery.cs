namespace Venue.TimeCafe.Application.CQRS.Promotions.Queries;

public record GetPromotionByIdQuery(int PromotionId) : IRequest<GetPromotionByIdResult>;

public record GetPromotionByIdResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Promotion? Promotion = null) : ICqrsResultV2
{
    public static GetPromotionByIdResult PromotionNotFound() =>
        new(false, Code: "PromotionNotFound", Message: "Акция не найдена", StatusCode: 404);

    public static GetPromotionByIdResult GetFailed() =>
        new(false, Code: "GetPromotionFailed", Message: "Не удалось получить акцию", StatusCode: 500);

    public static GetPromotionByIdResult GetSuccess(Promotion promotion) =>
        new(true, Promotion: promotion);
}

public class GetPromotionByIdQueryValidator : AbstractValidator<GetPromotionByIdQuery>
{
    public GetPromotionByIdQueryValidator()
    {
        RuleFor(x => x.PromotionId)
            .GreaterThan(0).WithMessage("ID акции обязателен");
    }
}

public class GetPromotionByIdQueryHandler(IPromotionRepository repository) : IRequestHandler<GetPromotionByIdQuery, GetPromotionByIdResult>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<GetPromotionByIdResult> Handle(GetPromotionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var promotion = await _repository.GetByIdAsync(request.PromotionId);

            if (promotion == null)
                return GetPromotionByIdResult.PromotionNotFound();

            return GetPromotionByIdResult.GetSuccess(promotion);
        }
        catch (Exception)
        {
            return GetPromotionByIdResult.GetFailed();
        }
    }
}
