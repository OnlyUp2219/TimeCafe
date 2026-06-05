namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record ForceEndVisitCommand(Guid VisitId) : ICommand<ForceEndVisitResponse>;

public class ForceEndVisitCommandValidator : AbstractValidator<ForceEndVisitCommand>
{
    public ForceEndVisitCommandValidator()
    {
        RuleFor(x => x.VisitId).ValidGuidEntityId("Посещение не найдено");
    }
}

public record ForceEndVisitResponse(Guid VisitId);

public class ForceEndVisitCommandHandler(
    IUnitOfWork uow,
    IPublishEndpoint publishEndpoint,
    IPublisher publisher,
    IOptionsSnapshot<VenuePricingOptions> options) : ICommandHandler<ForceEndVisitCommand, ForceEndVisitResponse>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly IPublisher _publisher = publisher;
    private readonly VenuePricingOptions _options = options.Value;

    public async Task<Result<ForceEndVisitResponse>> Handle(ForceEndVisitCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _uow.Visits.GetWithTariffByIdAsync(request.VisitId, cancellationToken);
            if (existing == null)
                return Result.Fail(new VisitNotFoundError());

            var exitTime = DateTimeOffset.UtcNow;
            var activePromotions = await _uow.Promotions.GetActiveByDateAsync(exitTime, cancellationToken);
            var globalDiscount = activePromotions.Where(p => p.Type == PromotionType.Global).Max(p => p.DiscountPercent) ?? 0m;
            var tariffDiscount = activePromotions.Where(p => p.Type == PromotionType.TariffSpecific && p.TariffId == existing.TariffId).Max(p => p.DiscountPercent) ?? 0m;

            var personalDiscount = 0m;
            if (existing.UserId.HasValue)
            {
                var userLoyalty = await _uow.UserLoyalties.GetByUserIdAsync(existing.UserId.Value, cancellationToken);
                personalDiscount = userLoyalty?.PersonalDiscountPercent ?? 0m;
            }

            var breakdown = Visit.CalculateCost(
                    tariffBillingType: existing.TariffBillingType,
                    tariffPricePerMinute: existing.TariffPricePerMinute,
                    exitTime: exitTime,
                    entryTime: existing.EntryTime,
                    minSessionMinutes: existing.TariffMinSessionMinutes,
                    roundingRule: existing.TariffRoundingRule,
                    maxDiscountPercent: _options.MaxTotalDiscountPercent,
                    globalDiscount: globalDiscount,
                    tariffDiscount: tariffDiscount,
                    personalDiscount: personalDiscount);

            var entity = await _uow.Visits.GetByIdAsync(request.VisitId, cancellationToken);
            if (entity == null) return Result.Fail(new VisitNotFoundError());

            var result = entity.FixateTime(breakdown.FinalCost, exitTime);
            if (result.IsFailed)
                return Result.Fail(result.Errors);

            var updated = await _uow.Visits.UpdateAsync(entity, cancellationToken);
            if (updated == null)
                return Result.Fail(new EndVisitFailedError());

            await _publishEndpoint.Publish(new VisitTimerStoppedEvent
            {
                VisitId = updated.VisitId,
                UserId = updated.UserId,
                Amount = updated.CalculatedCost ?? 0,
                StoppedAt = exitTime
            }, cancellationToken);

            await _uow.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new VisitChangedEvent(updated.VisitId, updated.UserId), cancellationToken);

            return Result.Ok(new ForceEndVisitResponse(updated.VisitId));
        }
        catch (Exception ex)
        {
            return Result.Fail(new EndVisitFailedError().CausedBy(ex));
        }
    }
}
