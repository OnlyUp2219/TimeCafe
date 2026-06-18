namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record RequestEndVisitCommand(Guid VisitId, Guid UserId, bool PayFromBalance = false) : ICommand<RequestEndVisitResponse>;

public class RequestEndVisitCommandValidator : AbstractValidator<RequestEndVisitCommand>
{
    public RequestEndVisitCommandValidator()
    {
        RuleFor(x => x.VisitId).ValidGuidEntityId("Посещение не найдено");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Идентификатор пользователя обязателен");
    }
}

public record RequestEndVisitResponse(Guid VisitId);

public class RequestEndVisitCommandHandler(
    IUnitOfWork uow,
    IPublisher publisher) : ICommandHandler<RequestEndVisitCommand, RequestEndVisitResponse>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<RequestEndVisitResponse>> Handle(RequestEndVisitCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _uow.Visits.GetByIdAsync(request.VisitId, cancellationToken);
            if (existing == null)
                return Result.Fail(new VisitNotFoundError());

            if (existing.UserId != request.UserId)
                return Result.Fail(new VisitAccessDeniedError());

            var result = existing.RequestFinish(request.PayFromBalance);
            if (result.IsFailed)
                return Result.Fail(result.Errors);

            var updated = await _uow.Visits.UpdateAsync(existing, cancellationToken);
            if (updated == null)
                return Result.Fail(new EndVisitFailedError());

            await _uow.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new VisitChangedEvent(updated.VisitId, updated.UserId), cancellationToken);

            return Result.Ok(new RequestEndVisitResponse(updated.VisitId));
        }
        catch (Exception ex)
        {
            return Result.Fail(new EndVisitFailedError().CausedBy(ex));
        }
    }
}
