namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record UpdatePromotionCommand(Guid PromotionId, string Name, string Description, decimal? DiscountPercent, DateTimeOffset ValidFrom, DateTimeOffset ValidTo, bool IsActive) : IRequest<UpdatePromotionResult>;

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
        RuleFor(x => x.PromotionId).ValidGuidEntityId("Акция не найдена");

        RuleFor(x => x.Name).ValidName("Название акции", 200);

        RuleFor(x => x.Description).ValidDescription(1000);

        RuleFor(x => x.DiscountPercent).ValidDiscountPercent();

        RuleFor(x => x.ValidFrom).ValidFromBeforeValidTo(x => x.ValidTo);
    }
}

public class UpdatePromotionCommandHandler(IPromotionRepository repository) : IRequestHandler<UpdatePromotionCommand, UpdatePromotionResult>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<UpdatePromotionResult> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.PromotionId);
            if (existing == null)
                return UpdatePromotionResult.PromotionNotFound();

            var promotion = Promotion.Update(
                existingPromotion: existing,
                name: request.Name,
                description: request.Description,
                discountPercent: request.DiscountPercent,
                validFrom: request.ValidFrom,
                validTo: request.ValidTo,
                isActive: request.IsActive
                );

            var updated = await _repository.UpdateAsync(promotion);

            if (updated == null)
                return UpdatePromotionResult.UpdateFailed();

            return UpdatePromotionResult.UpdateSuccess(updated);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(UpdatePromotionResult.UpdateFailed(), ex);
        }
    }
}
