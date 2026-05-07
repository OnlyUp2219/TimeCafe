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

public class UpdatePromotionCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<UpdatePromotionCommand, Promotion>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<Promotion>> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _uow.Promotions.GetByIdAsync(request.PromotionId, cancellationToken);
            if (existing == null)
                return Result.Fail(new PromotionNotFoundError());

            if ((request.Type == PromotionType.Global || request.Type == null && existing.Type == PromotionType.Global) && request.IsActive)
            {
                var activePromos = await _uow.Promotions.GetActiveAsync(cancellationToken);
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

            var updated = await _uow.Promotions.UpdateAsync(promotion, cancellationToken);

            if (updated == null)
                return Result.Fail(new UpdateFailedError());

            await _uow.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new PromotionChangedEvent(updated.PromotionId), cancellationToken);

            return Result.Ok(updated);
        }
        catch (Exception ex)
        {
            return Result.Fail(new UpdateFailedError().CausedBy(ex));
        }
    }
}

