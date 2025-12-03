namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record UpdatePromotionCommand(Promotion Promotion) : IRequest<UpdatePromotionResult>;

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
        RuleFor(x => x.Promotion)
            .NotNull().WithMessage("Акция обязательна");

        When(x => x.Promotion != null, () =>
        {
            RuleFor(x => x.Promotion.PromotionId)
                .GreaterThan(0).WithMessage("ID акции обязателен");

            RuleFor(x => x.Promotion.Name)
                .NotEmpty().WithMessage("Название акции обязательно")
                .MaximumLength(200).WithMessage("Название не может превышать 200 символов");

            RuleFor(x => x.Promotion.Description)
                .NotEmpty().WithMessage("Описание акции обязательно")
                .MaximumLength(1000).WithMessage("Описание не может превышать 1000 символов");

            RuleFor(x => x.Promotion.DiscountPercent)
                .GreaterThan(0).WithMessage("Процент скидки должен быть больше 0")
                .LessThanOrEqualTo(100).WithMessage("Процент скидки не может превышать 100")
                .When(x => x.Promotion.DiscountPercent.HasValue);

            RuleFor(x => x.Promotion.ValidFrom)
                .LessThan(x => x.Promotion.ValidTo).WithMessage("Дата начала должна быть раньше даты окончания");
        });
    }
}

public class UpdatePromotionCommandHandler(IPromotionRepository repository) : IRequestHandler<UpdatePromotionCommand, UpdatePromotionResult>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<UpdatePromotionResult> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.Promotion.PromotionId);
            if (existing == null)
                return UpdatePromotionResult.PromotionNotFound();

            var updated = await _repository.UpdateAsync(request.Promotion);

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
