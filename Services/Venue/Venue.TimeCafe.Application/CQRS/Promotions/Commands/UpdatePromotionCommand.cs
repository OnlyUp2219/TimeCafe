namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record UpdatePromotionCommand(Guid PromotionId, string Name, string Description, decimal? DiscountPercent, DateTimeOffset ValidFrom, DateTimeOffset ValidTo, bool IsActive, PromotionType? Type = null, Guid? TariffId = null) : ICommand<Promotion>;

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

public class UpdatePromotionCommandHandler(IPromotionRepository repository) : ICommandHandler<UpdatePromotionCommand, Promotion>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<Result<Promotion>> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.PromotionId);
            if (existing == null)
                return Result.Fail(new Error("Акция не найдена").WithMetadata("ErrorCode", "404"));

            if ((request.Type == PromotionType.Global || (request.Type == null && existing.Type == PromotionType.Global)) && request.IsActive)
            {
                var activePromos = await _repository.GetActiveAsync(cancellationToken);
                if (activePromos.Any(p => p.Type == PromotionType.Global && p.PromotionId != request.PromotionId))
                {
                    return Result.Fail(new ActiveGlobalPromotionAlreadyExistsError());
                }
            }

            var promotionResult = Promotion.Update(
                existingPromotion: existing,
                name: request.Name,
                description: request.Description,
                DiscountPercent: request.DiscountPercent,
                validFrom: request.ValidFrom,
                validTo: request.ValidTo,
                isActive: request.IsActive,
                type: request.Type,
                tariffId: request.TariffId
                );

            if (promotionResult.IsFailed)
                return Result.Fail(promotionResult.Errors);

            var promotion = promotionResult.Value;

            var updated = await _repository.UpdateAsync(promotion);

            if (updated == null)
                return Result.Fail(new Error("Не удалось обновить акцию").WithMetadata("ErrorCode", "500"));

            return Result.Ok(updated);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Не удалось обновить акцию").CausedBy(ex).WithMetadata("ErrorCode", "500"));
        }
    }
}

