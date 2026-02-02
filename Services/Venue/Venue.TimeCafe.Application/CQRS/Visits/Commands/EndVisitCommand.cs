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

public class EndVisitCommandHandler(IVisitRepository repository, IMapper mapper, IPublishEndpoint publishEndpoint) : IRequestHandler<EndVisitCommand, EndVisitResult>
{
    private readonly IVisitRepository _repository = repository;
    private readonly IMapper _mapper = mapper;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

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

            // Publish VisitCompletedEvent with timeout to avoid hanging
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(5)); // 5 second timeout for publish

                await _publishEndpoint.Publish(new VisitCompletedEvent
                {
                    VisitId = updated.VisitId,
                    UserId = updated.UserId,
                    Amount = visit.CalculatedCost ?? 0,
                    CompletedAt = exitTime
                }, cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Log but don't fail the request - event publishing timeout is not critical
                // The visit was updated successfully, message bus will retry
            }

            return EndVisitResult.EndSuccess(updated, visit.CalculatedCost ?? 0);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(EndVisitResult.EndFailed(), ex);
        }
    }
}
