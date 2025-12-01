namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record EndVisitCommand(int VisitId) : IRequest<EndVisitResult>;

public record EndVisitResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Visit? Visit = null,
    decimal? CalculatedCost = null) : ICqrsResultV2
{
    public static EndVisitResult VisitNotFound() =>
        new(false, Code: "VisitNotFound", Message: "Посещение не найдено", StatusCode: 404);

    public static EndVisitResult EndFailed() =>
        new(false, Code: "EndVisitFailed", Message: "Не удалось завершить посещение", StatusCode: 500);

    public static EndVisitResult EndSuccess(Visit visit, decimal calculatedCost) =>
        new(true, Message: "Посещение успешно завершено", Visit: visit, CalculatedCost: calculatedCost);
}

public class EndVisitCommandValidator : AbstractValidator<EndVisitCommand>
{
    public EndVisitCommandValidator()
    {
        RuleFor(x => x.VisitId)
            .GreaterThan(0).WithMessage("ID посещения обязателен");
    }
}

public class EndVisitCommandHandler(IVisitRepository repository) : IRequestHandler<EndVisitCommand, EndVisitResult>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<EndVisitResult> Handle(EndVisitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = await _repository.GetByIdAsync(request.VisitId);
            if (visit == null)
                return EndVisitResult.VisitNotFound();

            visit.ExitTime = DateTime.UtcNow;
            visit.Status = VisitStatus.Completed;

            var duration = (visit.ExitTime.Value - visit.EntryTime).TotalMinutes;

            if (visit.Tariff != null)
            {
                visit.CalculatedCost = visit.Tariff.BillingType == BillingType.Hourly
                    ? (decimal)Math.Ceiling(duration / 60) * (visit.Tariff.PricePerMinute * 60)
                    : (decimal)Math.Ceiling(duration) * visit.Tariff.PricePerMinute;
            }

            var updated = await _repository.UpdateAsync(visit);

            if (updated == null)
                return EndVisitResult.EndFailed();

            return EndVisitResult.EndSuccess(updated, visit.CalculatedCost ?? 0);
        }
        catch (Exception)
        {
            return EndVisitResult.EndFailed();
        }
    }
}
