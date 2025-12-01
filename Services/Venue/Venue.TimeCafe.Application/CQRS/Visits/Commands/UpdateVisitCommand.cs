namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record UpdateVisitCommand(Visit Visit) : IRequest<UpdateVisitResult>;

public record UpdateVisitResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Visit? Visit = null) : ICqrsResultV2
{
    public static UpdateVisitResult VisitNotFound() =>
        new(false, Code: "VisitNotFound", Message: "Посещение не найдено", StatusCode: 404);

    public static UpdateVisitResult UpdateFailed() =>
        new(false, Code: "UpdateVisitFailed", Message: "Не удалось обновить посещение", StatusCode: 500);

    public static UpdateVisitResult UpdateSuccess(Visit visit) =>
        new(true, Message: "Посещение успешно обновлено", Visit: visit);
}

public class UpdateVisitCommandValidator : AbstractValidator<UpdateVisitCommand>
{
    public UpdateVisitCommandValidator()
    {
        RuleFor(x => x.Visit)
            .NotNull().WithMessage("Посещение обязательно");

        RuleFor(x => x.Visit.VisitId)
            .GreaterThan(0).WithMessage("ID посещения обязателен");

        RuleFor(x => x.Visit.UserId)
            .NotEmpty().WithMessage("ID пользователя обязателен")
            .MaximumLength(450).WithMessage("ID пользователя не может превышать 450 символов");

        RuleFor(x => x.Visit.TariffId)
            .GreaterThan(0).WithMessage("ID тарифа обязателен");
    }
}

public class UpdateVisitCommandHandler(IVisitRepository repository) : IRequestHandler<UpdateVisitCommand, UpdateVisitResult>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<UpdateVisitResult> Handle(UpdateVisitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.Visit.VisitId);
            if (existing == null)
                return UpdateVisitResult.VisitNotFound();

            var updated = await _repository.UpdateAsync(request.Visit);

            if (updated == null)
                return UpdateVisitResult.UpdateFailed();

            return UpdateVisitResult.UpdateSuccess(updated);
        }
        catch (Exception)
        {
            return UpdateVisitResult.UpdateFailed();
        }
    }
}
