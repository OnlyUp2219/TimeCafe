namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record CreatePromotionCommand(
    string Name,
    string Description,
    decimal? DiscountPercent,
    DateTime ValidFrom,
    DateTime ValidTo,
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
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название акции обязательно")
            .MaximumLength(200).WithMessage("Название не может превышать 200 символов");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Описание акции обязательно")
            .MaximumLength(1000).WithMessage("Описание не может превышать 1000 символов");

        RuleFor(x => x.DiscountPercent)
            .GreaterThan(0).WithMessage("Процент скидки должен быть больше 0")
            .LessThanOrEqualTo(100).WithMessage("Процент скидки не может превышать 100")
            .When(x => x.DiscountPercent.HasValue);

        RuleFor(x => x.ValidFrom)
            .LessThan(x => x.ValidTo).WithMessage("Дата начала должна быть раньше даты окончания");
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
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(promotion);

            if (created == null)
                return CreatePromotionResult.CreateFailed();

            return CreatePromotionResult.CreateSuccess(created);
        }
        catch (Exception)
        {
            return CreatePromotionResult.CreateFailed();
        }
    }
}
