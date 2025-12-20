namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record DeleteVisitCommand(string VisitId) : IRequest<DeleteVisitResult>;

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
            .NotEmpty().WithMessage("Посещение не найдено")
           .NotNull().WithMessage("Посещение не найдено")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Посещение не найдено");
    }
}

public class DeleteVisitCommandHandler(IVisitRepository repository) : IRequestHandler<DeleteVisitCommand, DeleteVisitResult>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<DeleteVisitResult> Handle(DeleteVisitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var visitId = Guid.Parse(request.VisitId);

            var existing = await _repository.GetByIdAsync(visitId);
            if (existing == null)
                return DeleteVisitResult.VisitNotFound();

            var result = await _repository.DeleteAsync(visitId);

            if (!result)
                return DeleteVisitResult.DeleteFailed();

            return DeleteVisitResult.DeleteSuccess();
        }
        catch (Exception)
        {
            return DeleteVisitResult.DeleteFailed();
        }
    }
}
