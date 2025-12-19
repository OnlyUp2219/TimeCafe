using Venue.TimeCafe.Application.Contracts.Repositories;

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
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Посещение не найдено")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Посещение не найдено");
    }
}

public class EndVisitCommandHandler(IVisitRepository repository) : IRequestHandler<EndVisitCommand, EndVisitResult>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<EndVisitResult> Handle(EndVisitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var visitId = Guid.Parse(request.VisitId);

            var visitDto = await _repository.GetByIdAsync(visitId);
            if (visitDto == null)
                return EndVisitResult.VisitNotFound();

            visitDto.ExitTime = DateTimeOffset.UtcNow;
            visitDto.Status = VisitStatus.Completed;

            var duration = (visitDto.ExitTime.Value - visitDto.EntryTime).TotalMinutes;

            if (visitDto.TariffId != null)
            {
                visitDto.CalculatedCost = visitDto.TariffBillingType == BillingType.Hourly
                    ? (decimal)Math.Ceiling(duration / 60) * (visitDto.TariffPricePerMinute * 60)
                    : (decimal)Math.Ceiling(duration) * visitDto.TariffPricePerMinute;
            }

            //TODO : AutoMapper
            var visit = new Visit(visitDto.VisitId)
            {
                UserId = visitDto.UserId,
                TariffId = visitDto.TariffId,
                EntryTime = visitDto.EntryTime,
                ExitTime = visitDto.ExitTime,
                CalculatedCost = visitDto.CalculatedCost,
                Status = visitDto.Status
            };

            var updated = await _repository.UpdateAsync(visit);

            if (updated == null)
                return EndVisitResult.EndFailed();

            return EndVisitResult.EndSuccess(updated, visitDto.CalculatedCost ?? 0);
        }
        catch (Exception)
        {
            return EndVisitResult.EndFailed();
        }
    }
}
