namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record CreatePromotionCommand(
    string Name,
    string Description,
    decimal? DiscountPercent,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    bool IsActive = true) : IRequest<CreatePromotionResult>;

public record CreatePromotionResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Promotion? Promotion = null) : ICqrsResultV2
{
    public static CreatePromotionResult CreateFailed() =>
        new(false, Code: "CreatePromotionFailed", Message: "Не удалось создать акцию", StatusCode: 500);

    public static CreatePromotionResult CreateSuccess(Promotion promotion) =>
        new(true, Message: "Акция успешно создана", StatusCode: 201, Promotion: promotion);
}

public class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionCommandValidator()
    {
        RuleFor(x => x.Name).ValidName("Название акции", 200);

        RuleFor(x => x.Description).ValidDescription(1000);

        RuleFor(x => x.DiscountPercent).ValidDiscountPercent()
            .When(x => x.DiscountPercent.HasValue);

        RuleFor(x => x.ValidFrom).ValidFromBeforeValidTo(x => x.ValidTo);
    }
}

public class CreatePromotionCommandHandler(IPromotionRepository repository) : IRequestHandler<CreatePromotionCommand, CreatePromotionResult>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<CreatePromotionResult> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var promotion = new Promotion
            {
                Name = request.Name,
                Description = request.Description,
                DiscountPercent = request.DiscountPercent,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
                IsActive = request.IsActive,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var created = await _repository.CreateAsync(promotion);

            if (created == null)
                return CreatePromotionResult.CreateFailed();

            return CreatePromotionResult.CreateSuccess(created);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(CreatePromotionResult.CreateFailed(), ex);
        }
    }
}
