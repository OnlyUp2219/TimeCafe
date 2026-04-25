namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record DeleteVisitCommand(Guid VisitId) : ICommand;

public class DeleteVisitCommandValidator : AbstractValidator<DeleteVisitCommand>
{
    public DeleteVisitCommandValidator()
    {
        RuleFor(x => x.VisitId).ValidGuidEntityId("Посещение не найдено");
    }
}

public class DeleteVisitCommandHandler(IVisitRepository repository) : ICommandHandler<DeleteVisitCommand>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<Result> Handle(DeleteVisitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.VisitId);
            if (existing == null)
                return Result.Fail(new VisitNotFoundError());

            var result = await _repository.DeleteAsync(request.VisitId);

            if (!result)
                return Result.Fail(new DeleteFailedError());

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

