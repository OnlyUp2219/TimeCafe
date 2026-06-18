namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record FixateVisitTimeCommand(Guid VisitId) : ICommand<FixateVisitTimeResponse>;

public record FixateVisitTimeResponse(Visit Visit, decimal CalculatedCost, CostBreakdownDto Breakdown);

public class FixateVisitTimeCommandValidator : AbstractValidator<FixateVisitTimeCommand>
{
    public FixateVisitTimeCommandValidator()
    {
        RuleFor(x => x.VisitId).ValidGuidEntityId("Посещение не найдено");
    }
}

public class FixateVisitTimeCommandHandler(
    IUnitOfWork uow,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IPublisher publisher,
    IOptionsSnapshot<VenuePricingOptions> options) : ICommandHandler<FixateVisitTimeCommand, FixateVisitTimeResponse>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IMapper _mapper = mapper;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly IPublisher _publisher = publisher;
    private readonly VenuePricingOptions _options = options.Value;

    public async Task<Result<FixateVisitTimeResponse>> Handle(FixateVisitTimeCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _uow.Visits.GetWithTariffByIdAsync(request.VisitId, cancellationToken);
            if (existing == null)
                return Result.Fail(new VisitNotFoundError());

            var exitTime = DateTimeOffset.UtcNow;
            if (existing.IsFinishRequested && existing.FinishRequestedAt.HasValue &&
                (DateTimeOffset.UtcNow - existing.FinishRequestedAt.Value).TotalMinutes <= _options.GracePeriodMinutes)
            {
                exitTime = existing.FinishRequestedAt.Value;
            }

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
                StoppedAt = exitTime,
                PayFromBalance = updated.PayFromBalance
            }, cancellationToken);

            await _uow.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new VisitChangedEvent(updated.VisitId, updated.UserId), cancellationToken);

            var breakdownDto = new CostBreakdownDto
            {
                ActualMinutes = breakdown.ActualMinutes,
                BillableMinutes = breakdown.BillableMinutes,
                BaseCost = breakdown.BaseCost,
                FinalCost = breakdown.FinalCost,
                OptimizationGain = breakdown.OptimizationGain
            };

            return Result.Ok(new FixateVisitTimeResponse(updated, updated.CalculatedCost ?? 0, breakdownDto));
        }
        catch (Exception ex)
        {
            return Result.Fail(new EndVisitFailedError().CausedBy(ex));
        }
    }
}

