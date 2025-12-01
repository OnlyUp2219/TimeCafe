namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record DeactivateTariffCommand(int TariffId) : IRequest<DeactivateTariffResult>;

public record DeactivateTariffResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static DeactivateTariffResult TariffNotFound() =>
        new(false, Code: "TariffNotFound", Message: "Тариф не найден", StatusCode: 404);

    public static DeactivateTariffResult DeactivateFailed() =>
        new(false, Code: "DeactivateTariffFailed", Message: "Не удалось деактивировать тариф", StatusCode: 500);

    public static DeactivateTariffResult DeactivateSuccess() =>
        new(true, Message: "Тариф успешно деактивирован");
}

public class DeactivateTariffCommandValidator : AbstractValidator<DeactivateTariffCommand>
{
    public DeactivateTariffCommandValidator()
    {
        RuleFor(x => x.TariffId)
            .GreaterThan(0).WithMessage("ID тарифа обязателен");
    }
}

public class DeactivateTariffCommandHandler(ITariffRepository repository) : IRequestHandler<DeactivateTariffCommand, DeactivateTariffResult>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<DeactivateTariffResult> Handle(DeactivateTariffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repository.DeactivateAsync(request.TariffId);

            if (!result)
                return DeactivateTariffResult.TariffNotFound();

            return DeactivateTariffResult.DeactivateSuccess();
        }
        catch (Exception)
        {
            return DeactivateTariffResult.DeactivateFailed();
        }
    }
}
