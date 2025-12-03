namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record ActivateTariffCommand(int TariffId) : IRequest<ActivateTariffResult>;

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
            .GreaterThan(0).WithMessage("ID тарифа обязателен");
    }
}

public class ActivateTariffCommandHandler(ITariffRepository repository) : IRequestHandler<ActivateTariffCommand, ActivateTariffResult>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<ActivateTariffResult> Handle(ActivateTariffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.TariffId);
            if (existing == null)
                return ActivateTariffResult.TariffNotFound();

            var result = await _repository.ActivateAsync(request.TariffId);

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
