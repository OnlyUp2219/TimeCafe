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
    IPublisher publisher) : ICommandHandler<ForceEndVisitCommand, ForceEndVisitResponse>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<ForceEndVisitResponse>> Handle(ForceEndVisitCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _uow.Visits.GetByIdAsync(request.VisitId, cancellationToken);
            if (existing == null)
                return Result.Fail(new VisitNotFoundError());

            var result = existing.RequestFinish();
            if (result.IsFailed)
                return Result.Fail(result.Errors);

            var updated = await _uow.Visits.UpdateAsync(existing, cancellationToken);
            if (updated == null)
                return Result.Fail(new EndVisitFailedError());

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
