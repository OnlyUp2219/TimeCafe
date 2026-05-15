namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record EndVisitCommand(Guid VisitId) : ICommand<EndVisitResponse>;

public class EndVisitCommandValidator : AbstractValidator<EndVisitCommand>
{
    public EndVisitCommandValidator()
    {
        RuleFor(x => x.VisitId).ValidGuidEntityId("Посещение не найдено");
    }
}

public class EndVisitCommandHandler(
    IUnitOfWork uow,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IPublisher publisher,
    ILogger<EndVisitCommandHandler> logger,
    IOptionsSnapshot<VenuePricingOptions> options) : ICommandHandler<EndVisitCommand, EndVisitResponse>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IMapper _mapper = mapper;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly IPublisher _publisher = publisher;
    private readonly ILogger<EndVisitCommandHandler> _logger = logger;
    private readonly VenuePricingOptions _options = options.Value;

    public async Task<Result<EndVisitResponse>> Handle(EndVisitCommand request, CancellationToken cancellationToken = default)
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

            var userLoyalty = await _uow.UserLoyalties.GetByUserIdAsync(existing.UserId, cancellationToken);
            var personalDiscount = userLoyalty?.PersonalDiscountPercent ?? 0m;

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

            var visit = Visit.Update(
                existingVisit: _mapper.Map<Visit>(existing),
                exitTime: exitTime,
                calculatedCost: breakdown.FinalCost,
                status: VisitStatus.Completed);

            var updated = await _uow.Visits.UpdateAsync(visit, cancellationToken);
            if (updated == null)
                return Result.Fail(new EndVisitFailedError());

            await _publishEndpoint.Publish(new VisitCompletedEvent
            {
                VisitId = updated.VisitId,
                UserId = updated.UserId,
                Amount = visit.CalculatedCost ?? 0,
                CompletedAt = exitTime
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

            return Result.Ok(new EndVisitResponse(updated, visit.CalculatedCost ?? 0, breakdownDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при завершении визита {VisitId}", request.VisitId);
            return Result.Fail(new EndVisitFailedError().CausedBy(ex));
        }
    }
}

