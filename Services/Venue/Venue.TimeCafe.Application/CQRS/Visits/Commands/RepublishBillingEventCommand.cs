namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record RepublishBillingEventCommand(Guid VisitId) : ICommand;

public class RepublishBillingEventCommandValidator : AbstractValidator<RepublishBillingEventCommand>
{
    public RepublishBillingEventCommandValidator()
    {
        RuleFor(x => x.VisitId).ValidGuidEntityId("Посещение не найдено");
    }
}

public class RepublishBillingEventCommandHandler(
    IUnitOfWork uow,
    IPublishEndpoint publishEndpoint) : ICommandHandler<RepublishBillingEventCommand>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task<Result> Handle(RepublishBillingEventCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _uow.Visits.GetByIdAsync(request.VisitId, cancellationToken);
        if (entity == null) return Result.Fail(new VisitNotFoundError());

        if (entity.Status != VisitStatus.WaitingForPayment && entity.Status != VisitStatus.Completed)
            throw new InvalidVisitStatusException();

        if (!entity.CalculatedCost.HasValue)
            return Result.Fail(new VisitCostNotCalculatedError());

        await _publishEndpoint.Publish(new VisitTimerStoppedEvent
        {
            VisitId = entity.VisitId,
            UserId = entity.UserId,
            Amount = entity.CalculatedCost.Value,
            StoppedAt = entity.ExitTime ?? DateTimeOffset.UtcNow
        }, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
