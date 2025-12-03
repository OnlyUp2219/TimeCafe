namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record DeleteTariffCommand(int TariffId) : IRequest<DeleteTariffResult>;

public record DeleteTariffResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static DeleteTariffResult TariffNotFound() =>
        new(false, Code: "TariffNotFound", Message: "Тариф не найден", StatusCode: 404);

    public static DeleteTariffResult DeleteFailed() =>
        new(false, Code: "DeleteTariffFailed", Message: "Не удалось удалить тариф", StatusCode: 500);

    public static DeleteTariffResult DeleteSuccess() =>
        new(true, Message: "Тариф успешно удалён");
}

public class DeleteTariffCommandValidator : AbstractValidator<DeleteTariffCommand>
{
    public DeleteTariffCommandValidator()
    {
        RuleFor(x => x.TariffId)
            .GreaterThan(0).WithMessage("ID тарифа обязателен");
    }
}

public class DeleteTariffCommandHandler(ITariffRepository repository) : IRequestHandler<DeleteTariffCommand, DeleteTariffResult>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<DeleteTariffResult> Handle(DeleteTariffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.TariffId);
            if (existing == null)
                return DeleteTariffResult.TariffNotFound();

            var result = await _repository.DeleteAsync(request.TariffId);

            if (!result)
                return DeleteTariffResult.DeleteFailed();

            return DeleteTariffResult.DeleteSuccess();
        }
        catch (Exception)
        {
            return DeleteTariffResult.DeleteFailed();
        }
    }
}
