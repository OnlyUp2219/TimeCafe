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
    IVisitRepository repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    ILogger<EndVisitCommandHandler> logger,
    IPromotionRepository promotionRepository,
    IUserLoyaltyRepository userLoyaltyRepository,
    IOptionsSnapshot<VenuePricingOptions> options) : ICommandHandler<EndVisitCommand, EndVisitResponse>
{
    private readonly IVisitRepository _repository = repository;
    private readonly IMapper _mapper = mapper;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ILogger<EndVisitCommandHandler> _logger = logger;
    private readonly IPromotionRepository _promotionRepository = promotionRepository;
    private readonly IUserLoyaltyRepository _userLoyaltyRepository = userLoyaltyRepository;
    private readonly VenuePricingOptions _options = options.Value;

    public async Task<Result<EndVisitResponse>> Handle(EndVisitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.VisitId);
            if (existing == null)
                return Result.Fail(new VisitNotFoundError());

            var exitTime = DateTimeOffset.UtcNow;
            var activePromotions = await _promotionRepository.GetActiveByDateAsync(exitTime, cancellationToken);
            var globalDiscount = activePromotions.Where(p => p.Type == PromotionType.Global).Max(p => p.DiscountPercent) ?? 0m;
            var tariffDiscount = activePromotions.Where(p => p.Type == PromotionType.TariffSpecific && p.TariffId == existing.TariffId).Max(p => p.DiscountPercent) ?? 0m;

            var userLoyalty = await _userLoyaltyRepository.GetByUserIdAsync(existing.UserId, cancellationToken);
            var personalDiscount = userLoyalty?.PersonalDiscountPercent ?? 0m;

            var visit = Visit.Update(
                existingVisit: _mapper.Map<Visit>(existing),
                exitTime: exitTime,
                calculatedCost: Visit.CalculateCost(
                    tariffBillingType: existing.TariffBillingType,
                    tariffPricePerMinute: existing.TariffPricePerMinute,
                    exitTime: exitTime,
                    entryTime: existing.EntryTime,
                    maxDiscountPercent: _options.MaxTotalDiscountPercent,
                    globalDiscount: globalDiscount,
                    tariffDiscount: tariffDiscount,
                    personalDiscount: personalDiscount),
                status: VisitStatus.Completed);

            var updated = await _repository.UpdateAsync(visit, saveChanges: false, cancellationToken);
            if (updated == null)
                return Result.Fail(new EndVisitFailedError());

            await _publishEndpoint.Publish(new VisitCompletedEvent
            {
                VisitId = updated.VisitId,
                UserId = updated.UserId,
                Amount = visit.CalculatedCost ?? 0,
                CompletedAt = exitTime
            }, cancellationToken);

            await _repository.SaveChangesAsync(cancellationToken);

            return Result.Ok(new EndVisitResponse(updated, visit.CalculatedCost ?? 0));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при завершении визита {VisitId}", request.VisitId);
            return Result.Fail(new EndVisitFailedError().CausedBy(ex));
        }
    }
}

