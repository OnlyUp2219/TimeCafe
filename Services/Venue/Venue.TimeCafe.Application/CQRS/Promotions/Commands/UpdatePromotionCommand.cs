namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record UpdatePromotionCommand(string PromotionId, string Name, string Description, decimal? DiscountPercent, DateTimeOffset ValidFrom, DateTimeOffset ValidTo, bool IsActive) : IRequest<UpdatePromotionResult>;

public record UpdatePromotionResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Promotion? Promotion = null) : ICqrsResultV2
{
    public static UpdatePromotionResult PromotionNotFound() =>
        new(false, Code: "PromotionNotFound", Message: "Акция не найдена", StatusCode: 404);

    public static UpdatePromotionResult UpdateFailed() =>
        new(false, Code: "UpdatePromotionFailed", Message: "Не удалось обновить акцию", StatusCode: 500);

    public static UpdatePromotionResult UpdateSuccess(Promotion promotion) =>
        new(true, Message: "Акция успешно обновлена", Promotion: promotion);
}

public class UpdatePromotionCommandValidator : AbstractValidator<UpdatePromotionCommand>
{
    public UpdatePromotionCommandValidator()
    {
        RuleFor(x => x.PromotionId)
           .NotEmpty().WithMessage("Акция не найдена")
           .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Акция не найдена")
           .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Акция не найдена");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название акции обязательно")
            .MaximumLength(200).WithMessage("Название не может превышать 200 символов");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Описание акции обязательно")
            .MaximumLength(1000).WithMessage("Описание не может превышать 1000 символов");

        RuleFor(x => x.DiscountPercent)
            .GreaterThan(0).WithMessage("Процент скидки должен быть больше 0")
            .LessThanOrEqualTo(100).WithMessage("Процент скидки не может превышать 100");

        RuleFor(x => x.ValidFrom)
            .LessThan(x => x.ValidTo).WithMessage("Дата начала должна быть раньше даты окончания");
    }
}

public class UpdatePromotionCommandHandler(IPromotionRepository repository) : IRequestHandler<UpdatePromotionCommand, UpdatePromotionResult>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<UpdatePromotionResult> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var promotionId = Guid.Parse(request.PromotionId);

            var existing = await _repository.GetByIdAsync(promotionId);
            if (existing == null)
                return UpdatePromotionResult.PromotionNotFound();

            // TODO : AutoMapper
            var promotion = new Promotion(promotionId)
            {
                Name = request.Name,
                Description = request.Description,
                DiscountPercent = request.DiscountPercent,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
                IsActive = request.IsActive
            };

            var updated = await _repository.UpdateAsync(promotion);

            if (updated == null)
                return UpdatePromotionResult.UpdateFailed();

            return UpdatePromotionResult.UpdateSuccess(updated);
        }
        catch (Exception)
        {
            return UpdatePromotionResult.UpdateFailed();
        }
    }
}
