namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record CancelVisitCommand(Guid VisitId, Guid UserId) : ICommand;

public class CancelVisitCommandValidator : AbstractValidator<CancelVisitCommand>
{
    public CancelVisitCommandValidator()
    {
        RuleFor(x => x.VisitId).ValidGuidEntityId("Визит не найден");
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");
    }
}

public class CancelVisitCommandHandler(IUnitOfWork uow) : ICommandHandler<CancelVisitCommand>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result> Handle(CancelVisitCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var visit = await _uow.Visits.GetByIdAsync(request.VisitId, cancellationToken);
            if (visit is null)
                return Result.Fail(new VisitNotFoundError());

            if (visit.UserId != request.UserId)
                return Result.Fail(new Error("Вы можете отменить только свой визит.").WithMetadata("ErrorCode", "403"));

            var cancelResult = visit.Cancel();
            if (cancelResult.IsFailed)
                return cancelResult;

            var updated = await _uow.Visits.UpdateAsync(visit, cancellationToken);
            if (updated is null)
                return Result.Fail(new VisitUpdateFailedError());

            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
