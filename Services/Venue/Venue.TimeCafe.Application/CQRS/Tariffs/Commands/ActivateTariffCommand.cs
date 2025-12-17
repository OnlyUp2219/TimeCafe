namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record ActivateTariffCommand(string TariffId) : IRequest<ActivateTariffResult>;

public record ActivateTariffResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static ActivateTariffResult TariffNotFound() =>
        new(false, Code: "TariffNotFound", Message: "Тариф не найден", StatusCode: 404);

    public static ActivateTariffResult ActivateFailed() =>
        new(false, Code: "ActivateTariffFailed", Message: "Не удалось активировать тариф", StatusCode: 500);

    public static ActivateTariffResult ActivateSuccess() =>
        new(true, Message: "Тариф успешно активирован");
}

public class ActivateTariffCommandValidator : AbstractValidator<ActivateTariffCommand>
{
    public ActivateTariffCommandValidator()
    {
        RuleFor(x => x.TariffId)
            .NotEmpty().WithMessage("Тариф не найден")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Тариф не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Тариф не найден");
    }
}

public class ActivateTariffCommandHandler(ITariffRepository repository) : IRequestHandler<ActivateTariffCommand, ActivateTariffResult>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<ActivateTariffResult> Handle(ActivateTariffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tariffId = Guid.Parse(request.TariffId);

            var existing = await _repository.GetByIdAsync(tariffId);
            if (existing == null)
                return ActivateTariffResult.TariffNotFound();

            var result = await _repository.ActivateAsync(tariffId);

            if (!result)
                return ActivateTariffResult.ActivateFailed();

            return ActivateTariffResult.ActivateSuccess();
        }
        catch (Exception)
        {
            return ActivateTariffResult.ActivateFailed();
        }
    }
}
