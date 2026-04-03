namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record ActivateTariffCommand(Guid TariffId) : IRequest<ActivateTariffResult>;

public record ActivateTariffResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResult
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
        RuleFor(x => x.TariffId).ValidGuidEntityId("Тариф не найден");
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
        catch (Exception ex)
        {
            throw new CqrsResultException(ActivateTariffResult.ActivateFailed(), ex);
        }
    }
}
