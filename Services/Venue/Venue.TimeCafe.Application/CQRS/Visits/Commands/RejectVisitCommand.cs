namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record RejectVisitCommand(Guid VisitId, string Reason) : ICommand<Visit>;

public class RejectVisitCommandValidator : AbstractValidator<RejectVisitCommand>
{
    public RejectVisitCommandValidator()
    {
        RuleFor(x => x.VisitId).ValidGuidEntityId("Визит не найден");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Укажите причину отклонения").MaximumLength(500);
    }
}

public class RejectVisitCommandHandler(IUnitOfWork uow, IPublishEndpoint publishEndpoint, IPublisher publisher) : ICommandHandler<RejectVisitCommand, Visit>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<Visit>> Handle(RejectVisitCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var visit = await _uow.Visits.GetByIdAsync(request.VisitId, cancellationToken);
            if (visit is null)
                return Result.Fail(new VisitNotFoundError());

            var rejectResult = visit.Reject(request.Reason);
            if (rejectResult.IsFailed)
                return rejectResult;

            var updated = await _uow.Visits.UpdateAsync(visit, cancellationToken);
            if (updated is null)
                return Result.Fail(new VisitUpdateFailedError());

            await _uow.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new VisitChangedEvent(updated.VisitId, updated.UserId), cancellationToken);

            Venue.TimeCafe.Application.Metrics.VenueMetrics.PendingVisits.Dec();

            await _publishEndpoint.Publish(new VisitRejectedEvent
            {
                VisitId = updated.VisitId,
                UserId = updated.UserId,
                Reason = updated.RejectionReason!,
                RejectedAt = DateTimeOffset.UtcNow
            }, cancellationToken);

            return Result.Ok(updated);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
