namespace Venue.TimeCafe.Application.CQRS.Promotions.Queries;

public record GetPromotionByIdQuery(string PromotionId) : IRequest<GetPromotionByIdResult>;

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
            .NotEmpty().WithMessage("Акция не найдена")
           .NotNull().WithMessage("Акция не найдена")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Акция не найдена");
    }
}

public class GetPromotionByIdQueryHandler(IPromotionRepository repository) : IRequestHandler<GetPromotionByIdQuery, GetPromotionByIdResult>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<GetPromotionByIdResult> Handle(GetPromotionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var promotionId = Guid.Parse(request.PromotionId);

            var promotion = await _repository.GetByIdAsync(promotionId);

            if (promotion == null)
                return GetPromotionByIdResult.PromotionNotFound();

            return GetPromotionByIdResult.GetSuccess(promotion);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(GetPromotionByIdResult.GetFailed(), ex);
        }
    }
}
