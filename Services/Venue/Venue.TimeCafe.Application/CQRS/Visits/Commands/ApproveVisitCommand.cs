namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record ApproveVisitCommand(Guid VisitId, Guid ApprovedByUserId) : ICommand<Visit>;

public class ApproveVisitCommandValidator : AbstractValidator<ApproveVisitCommand>
{
    public ApproveVisitCommandValidator()
    {
        RuleFor(x => x.VisitId).ValidGuidEntityId("Визит не найден");
        RuleFor(x => x.ApprovedByUserId).ValidGuidEntityId("Пользователь не найден");
    }
}

public class ApproveVisitCommandHandler(IUnitOfWork uow, IPublishEndpoint publishEndpoint, IPublisher publisher) : ICommandHandler<ApproveVisitCommand, Visit>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<Visit>> Handle(ApproveVisitCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var visit = await _uow.Visits.GetByIdAsync(request.VisitId, cancellationToken);
            if (visit is null)
                return Result.Fail(new VisitNotFoundError());

            var approveResult = visit.Approve(request.ApprovedByUserId);
            if (approveResult.IsFailed)
                return approveResult;

            var updated = await _uow.Visits.UpdateAsync(visit, cancellationToken);
            if (updated is null)
                return Result.Fail(new VisitUpdateFailedError());

            await _uow.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new VisitChangedEvent(updated.VisitId, updated.UserId), cancellationToken);

            Venue.TimeCafe.Application.Metrics.VenueMetrics.ActiveVisits.Inc();
            Venue.TimeCafe.Application.Metrics.VenueMetrics.PendingVisits.Dec();

            await _publishEndpoint.Publish(new VisitApprovedEvent
            {
                VisitId = updated.VisitId,
                UserId = updated.UserId,
                ApprovedByUserId = updated.ApprovedByUserId!.Value,
                ApprovedAt = updated.ApprovedAt!.Value
            }, cancellationToken);

            return Result.Ok(updated);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
