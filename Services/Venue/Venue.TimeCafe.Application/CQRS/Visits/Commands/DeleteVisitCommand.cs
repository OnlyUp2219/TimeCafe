namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record DeleteVisitCommand(Guid VisitId) : ICommand;

public class DeleteVisitCommandValidator : AbstractValidator<DeleteVisitCommand>
{
    public DeleteVisitCommandValidator()
    {
        RuleFor(x => x.VisitId).ValidGuidEntityId("Посещение не найдено");
    }
}

public class DeleteVisitCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<DeleteVisitCommand>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result> Handle(DeleteVisitCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _uow.Visits.GetByIdAsync(request.VisitId, cancellationToken);
            if (existing == null)
                return Result.Fail(new VisitNotFoundError());

            var result = await _uow.Visits.DeleteAsync(request.VisitId, cancellationToken);

            if (!result)
                return Result.Fail(new DeleteFailedError());

            await _uow.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new VisitChangedEvent(existing.VisitId, existing.UserId), cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

