namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record WalkInVisitCommand(Guid TariffId, Guid? ResourceId = null, Guid? UserId = null, int GuestsCount = 1) : ICommand<WalkInVisitResponse>;

public class WalkInVisitCommandValidator : AbstractValidator<WalkInVisitCommand>
{
    public WalkInVisitCommandValidator()
    {
        RuleFor(x => x.TariffId).ValidGuidEntityId("Необходимо выбрать тариф");
        RuleFor(x => x.GuestsCount).GreaterThan(0).WithMessage("Количество гостей должно быть больше 0");
    }
}

public record WalkInVisitResponse(Guid VisitId, Guid TariffId, Guid? ResourceId, Guid? UserId, VisitStatus Status);

public class WalkInVisitCommandHandler(
    IUnitOfWork uow,
    IPublisher publisher) : ICommandHandler<WalkInVisitCommand, WalkInVisitResponse>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<WalkInVisitResponse>> Handle(WalkInVisitCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var tariff = await _uow.Tariffs.GetByIdAsync(request.TariffId, cancellationToken);
            if (tariff == null)
                return Result.Fail(new TariffNotFoundError());

            if (request.ResourceId.HasValue)
            {
                var isBusy = await _uow.Visits.IsResourceBusyAsync(request.ResourceId.Value, cancellationToken);
                if (isBusy)
                    return Result.Fail(new ResourceAlreadyInUseError());
            }

            var visit = Visit.Create(
                visitId: Guid.NewGuid(),
                userId: request.UserId,
                tariffId: request.TariffId,
                entryTime: DateTimeOffset.UtcNow,
                status: VisitStatus.Active,
                resourceId: request.ResourceId,
                guestsCount: request.GuestsCount
            );

            visit.ApprovedAt = DateTimeOffset.UtcNow;
            
            var created = await _uow.Visits.CreateAsync(visit, cancellationToken);
            if (created == null)
                return Result.Fail(new CreateVisitFailedError());

            await _uow.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(new VisitChangedEvent(created.VisitId, created.UserId), cancellationToken);

            Venue.TimeCafe.Application.Metrics.VenueMetrics.ActiveVisits.Inc();
            Venue.TimeCafe.Application.Metrics.VenueMetrics.VisitsStarted.Inc();

            return Result.Ok(new WalkInVisitResponse(created.VisitId, created.TariffId, created.ResourceId, created.UserId, created.Status));
        }
        catch (Exception ex)
        {
            return Result.Fail(new CreateVisitFailedError().CausedBy(ex));
        }
    }
}
