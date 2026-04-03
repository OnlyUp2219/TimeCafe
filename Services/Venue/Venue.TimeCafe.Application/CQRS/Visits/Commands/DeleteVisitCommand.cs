namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record DeleteVisitCommand(Guid VisitId) : IRequest<DeleteVisitResult>;

public record DeleteVisitResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResult
{
    public static DeleteVisitResult VisitNotFound() =>
        new(false, Code: "VisitNotFound", Message: "Посещение не найдено", StatusCode: 404);

    public static DeleteVisitResult DeleteFailed() =>
        new(false, Code: "DeleteVisitFailed", Message: "Не удалось удалить посещение", StatusCode: 500);

    public static DeleteVisitResult DeleteSuccess() =>
        new(true, Message: "Посещение успешно удалено");
}

public class DeleteVisitCommandValidator : AbstractValidator<DeleteVisitCommand>
{
    public DeleteVisitCommandValidator()
    {
        RuleFor(x => x.VisitId).ValidGuidEntityId("Посещение не найдено");
    }
}

public class DeleteVisitCommandHandler(IVisitRepository repository) : IRequestHandler<DeleteVisitCommand, DeleteVisitResult>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<DeleteVisitResult> Handle(DeleteVisitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.VisitId);
            if (existing == null)
                return DeleteVisitResult.VisitNotFound();

            var result = await _repository.DeleteAsync(request.VisitId);

            if (!result)
                return DeleteVisitResult.DeleteFailed();

            return DeleteVisitResult.DeleteSuccess();
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(DeleteVisitResult.DeleteFailed(), ex);
        }
    }
}
