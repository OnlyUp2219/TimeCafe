namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record EndVisitCommand(string VisitId) : IRequest<EndVisitResult>;

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
            .NotEmpty().WithMessage("Посещение не найдено")
           .NotNull().WithMessage("Посещение не найдено")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Посещение не найдено");
    }
}

public class EndVisitCommandHandler(IVisitRepository repository, IMapper mapper) : IRequestHandler<EndVisitCommand, EndVisitResult>
{
    private readonly IVisitRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<EndVisitResult> Handle(EndVisitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var visitId = Guid.Parse(request.VisitId);

            var existing = await _repository.GetByIdAsync(visitId);
            if (existing == null)
                return EndVisitResult.VisitNotFound();

            var exitTime = DateTimeOffset.UtcNow;
            var visit = Visit.Update(
                existingVisit: _mapper.Map<Visit>(existing),
                exitTime: exitTime,
                calculatedCost: Visit.CalculateCost(
                    tariffBillingType: existing.TariffBillingType,
                    tariffPricePerMinute: existing.TariffPricePerMinute,
                    exitTime: exitTime,
                    entryTime: existing.EntryTime),
                status: VisitStatus.Completed);

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
