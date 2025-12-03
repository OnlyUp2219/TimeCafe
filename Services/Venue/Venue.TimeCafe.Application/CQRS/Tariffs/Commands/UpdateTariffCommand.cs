namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record UpdateTariffCommand(Tariff Tariff) : IRequest<UpdateTariffResult>;

public record UpdateTariffResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Tariff? Tariff = null) : ICqrsResultV2
{
    public static UpdateTariffResult TariffNotFound() =>
        new(false, Code: "TariffNotFound", Message: "Тариф не найден", StatusCode: 404);

    public static UpdateTariffResult UpdateFailed() =>
        new(false, Code: "UpdateTariffFailed", Message: "Не удалось обновить тариф", StatusCode: 500);

    public static UpdateTariffResult UpdateSuccess(Tariff tariff) =>
        new(true, Message: "Тариф успешно обновлён", Tariff: tariff);
}

public class UpdateTariffCommandValidator : AbstractValidator<UpdateTariffCommand>
{
    public UpdateTariffCommandValidator()
    {
        RuleFor(x => x.Tariff)
            .NotNull().WithMessage("Тариф обязателен");

        When(x => x.Tariff != null, () =>
        {
            RuleFor(x => x.Tariff.TariffId)
                .GreaterThan(0).WithMessage("ID тарифа обязателен");

            RuleFor(x => x.Tariff.Name)
                .NotEmpty().WithMessage("Название тарифа обязательно")
                .MaximumLength(100).WithMessage("Название не может превышать 100 символов");

            RuleFor(x => x.Tariff.Description)
                .MaximumLength(500).WithMessage("Описание не может превышать 500 символов")
                .When(x => !string.IsNullOrEmpty(x.Tariff.Description));

            RuleFor(x => x.Tariff.PricePerMinute)
                .GreaterThan(0).WithMessage("Цена за минуту должна быть больше 0");
        });
    }
}

public class UpdateTariffCommandHandler(ITariffRepository repository) : IRequestHandler<UpdateTariffCommand, UpdateTariffResult>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<UpdateTariffResult> Handle(UpdateTariffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.Tariff.TariffId);
            if (existing == null)
                return UpdateTariffResult.TariffNotFound();

            var updated = await _repository.UpdateAsync(request.Tariff);

            if (updated == null)
                return UpdateTariffResult.UpdateFailed();

            return UpdateTariffResult.UpdateSuccess(updated);
        }
        catch (Exception)
        {
            return UpdateTariffResult.UpdateFailed();
        }
    }
}
