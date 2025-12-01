namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record DeleteVisitCommand(int VisitId) : IRequest<DeleteVisitResult>;

public record DeleteVisitResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
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
        RuleFor(x => x.VisitId)
            .GreaterThan(0).WithMessage("ID посещения обязателен");
    }
}

public class DeleteVisitCommandHandler(IVisitRepository repository) : IRequestHandler<DeleteVisitCommand, DeleteVisitResult>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<DeleteVisitResult> Handle(DeleteVisitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repository.DeleteAsync(request.VisitId);

            if (!result)
                return DeleteVisitResult.VisitNotFound();

            return DeleteVisitResult.DeleteSuccess();
        }
        catch (Exception)
        {
            return DeleteVisitResult.DeleteFailed();
        }
    }
}
